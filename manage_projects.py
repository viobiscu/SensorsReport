import os
import subprocess
import yaml
from sys import platform

# ANSI color codes
RED = '\033[91m'
GREEN = '\033[92m'
YELLOW = '\033[93m'
CYAN = '\033[96m'
RESET = '\033[0m'

# Project abbreviations
PROJECT_ABBREVIATIONS = {
    "Sensors-Report-Audit": "Audit",
    "Sensors-Report-Audit.API": "Audit.API",
    "Sensors-Report-Business-Broker.API": "Business-Broker.API",
    "Sensors-Report-MQTT-to-Orion": "MQTT-to-Orion",
    "Sensors-Report-Explorer": "Explorer"
}

# Docker image names for each project
DOCKER_IMAGE_NAMES = {
    "Sensors-Report-Audit": "viobiscu/sensors-report-audit:latest",
    "Sensors-Report-Audit.API": "viobiscu/sensors-report-audit:latest",
    "Sensors-Report-Business-Broker.API": "viobiscu/sensors-report-business-broker:latest",
    "Sensors-Report-MQTT-to-Orion": "viobiscu/sensors-report-mqtt-to-orion:latest",
    "Sensors-Report-Explorer": "viobiscu/sensors-report-explorer:latest"
}

# List all .csproj files in the workspace

def find_projects(root):
    projects = []
    for dirpath, _, files in os.walk(root):
        for file in files:
            if file.endswith('.csproj'):
                projects.append(os.path.join(dirpath, file))
    # Add static Explorer project (no .csproj, but has Dockerfile)
    explorer_path = os.path.join(root, 'Sensors-Report-Explorer')
    explorer_dockerfile = os.path.join(explorer_path, 'Dockerfile')
    if os.path.exists(explorer_dockerfile):
        projects.append(os.path.join(explorer_path, 'Dockerfile'))
    return projects

def print_error(msg):
    print(f"{RED}{msg}{RESET}")

def print_success(msg):
    print(f"{GREEN}{msg}{RESET}")

def print_warning(msg):
    print(f"{YELLOW}{msg}{RESET}")

def print_info(msg):
    print(f"{CYAN}{msg}{RESET}")

def build_project(csproj):
    print_info(f"Building {csproj} ...")
    result = subprocess.run(['dotnet', 'build', csproj])
    if result.returncode == 0:
        print_success("Build succeeded.")
    else:
        print_error("Build failed.")

def update_local_k8s(deployment_name, container_name, image_name, namespace="default"):
    print_info(f"Patching local k8s deployment {deployment_name} with image {image_name} ...")
    result = subprocess.run([
        "kubectl", "set", "image", f"deployment/{deployment_name}",
        f"{container_name}={image_name}", "-n", namespace
    ])
    if result.returncode == 0:
        print_success("Kubernetes deployment updated successfully.")
    else:
        print_error("Failed to update Kubernetes deployment. Make sure the deployment exists.")

def update_flux_manifest(manifest_path, new_image):
    import re
    print_info(f"Updating Flux manifest {manifest_path} with image {new_image} ...")
    with open(manifest_path, "r") as f:
        content = f.read()
    # Replace the image line
    new_content = re.sub(r'(image:\s*)([^\s]+)', f'\\1{new_image}', content)
    with open(manifest_path, "w") as f:
        f.write(new_content)
    subprocess.run(["git", "add", manifest_path])
    subprocess.run(["git", "commit", "-m", f"Update image to {new_image}"])
    subprocess.run(["git", "push"])
    print_success("Pushed manifest update to git. Flux will deploy the new version.")

def extract_container_name_from_deployment(deployment_yaml_path):
    try:
        with open(deployment_yaml_path, 'r') as f:
            deployment = yaml.safe_load(f)
        containers = deployment['spec']['template']['spec']['containers']
        if containers and 'name' in containers[0]:
            return containers[0]['name']
    except Exception as e:
        print(f"Could not extract container name from {deployment_yaml_path}: {e}")
    return None

def docker_build(csproj):
    project_dir = os.path.dirname(csproj)
    dockerfile = os.path.join(project_dir, 'Dockerfile')
    proj_dir = os.path.basename(project_dir)
    image_name = DOCKER_IMAGE_NAMES.get(proj_dir, f"viobiscu/{proj_dir.lower().replace('.', '-')}:latest")
    if os.path.exists(dockerfile):
        print_info(f"Building Docker image for {csproj} ...")
        workspace_root = os.path.dirname(os.path.abspath(__file__))
        dockerfile_rel = os.path.relpath(dockerfile, workspace_root)
        result = subprocess.run([
            'docker', 'build', '-f', dockerfile_rel, '-t', image_name, '.'
        ], cwd=workspace_root)
        if result.returncode == 0:
            print_success("Docker build succeeded.")
            print(f"{CYAN}Options for update:{RESET}")
            print(f"{CYAN}1. Update local k8s{RESET}")
            print(f"{CYAN}2. Update production via Flux{RESET}")
            print(f"{CYAN}b. Back to menu.{RESET}")
            choice = input(f"{YELLOW}Select an update option: {RESET}")
            if choice == "1":
                # Try to find the deployment YAML for this project
                flux_dir = os.path.join(workspace_root, 'flux')
                deployment_yaml = None
                service_yaml = None
                for subdir in os.listdir(flux_dir):
                    subdir_path = os.path.join(flux_dir, subdir)
                    if os.path.isdir(subdir_path):
                        candidate_deploy = os.path.join(subdir_path, 'deployment-test.yaml')
                        candidate_service = os.path.join(subdir_path, 'service.yaml')
                        if os.path.exists(candidate_deploy) and proj_dir.replace('.', '-').lower() in candidate_deploy:
                            deployment_yaml = candidate_deploy
                        if os.path.exists(candidate_service) and proj_dir.replace('.', '-').lower() in candidate_service:
                            service_yaml = candidate_service
                # Apply deployment and service YAMLs if found
                if deployment_yaml:
                    print_info(f"Applying {deployment_yaml} ...")
                    subprocess.run(["kubectl", "apply", "-f", deployment_yaml])
                else:
                    print_warning("Could not find deployment-test.yaml for this project in flux directory.")
                if service_yaml:
                    print_info(f"Applying {service_yaml} ...")
                    subprocess.run(["kubectl", "apply", "-f", service_yaml])
                else:
                    print_warning("Could not find service.yaml for this project in flux directory.")
                # Extract container and deployment name for patching
                if deployment_yaml:
                    container_name = extract_container_name_from_deployment(deployment_yaml)
                    deployment_name = None
                    try:
                        with open(deployment_yaml, 'r') as f:
                            deployment = yaml.safe_load(f)
                        deployment_name = deployment['metadata']['name']
                    except Exception as e:
                        print_error(f"Could not extract deployment name: {e}")
                    if container_name and deployment_name:
                        update_local_k8s(deployment_name, container_name, image_name)
                    else:
                        print_error("Could not determine deployment or container name from YAML.")
            elif choice == "2":
                manifest_path = input(f"{CYAN}Enter path to flux deployment manifest: {RESET}")
                update_flux_manifest(manifest_path, image_name)
            elif choice.lower() == 'b':
                print_info("Returning to menu.")
            else:
                print_warning("Invalid action.")
        else:
            print_error("Docker build failed.")
    else:
        print_error(f"No Dockerfile found for {csproj}")

def start_debug(csproj):
    project_dir = os.path.dirname(csproj)
    print_info(f"Starting debug for {csproj} ...")
    try:
        proc = subprocess.Popen(['dotnet', 'run', '--project', csproj, '--configuration', 'Debug'])
        input(f"{YELLOW}Press Enter to stop debugging and return to the menu...\n{RESET}")
        proc.terminate()
        proc.wait()
        print_success("Debugging stopped.")
    except KeyboardInterrupt:
        print_warning("\nDebugging stopped by user.")
    except Exception as e:
        print_error(f"Error during debug: {e}")

def main():
    root = os.path.dirname(os.path.abspath(__file__))
    projects = find_projects(root)
    if not projects:
        print_error("No .csproj files found.")
        return

    while True:
        for idx, proj in enumerate(projects):
            proj_dir = os.path.basename(os.path.dirname(proj))
            abbr = PROJECT_ABBREVIATIONS.get(proj_dir, proj_dir)
            print(f"{CYAN}{idx+1}. {abbr}{RESET}")

        choice = input(f"{YELLOW}Select a project by number (or 'q' to quit): {RESET}")
        if choice.lower() == 'q':
            break
        try:
            idx = int(choice) - 1
            if idx < 0 or idx >= len(projects):
                print_warning("Invalid selection.")
                continue
            csproj = projects[idx]
            proj_dir = os.path.basename(os.path.dirname(csproj))
            abbr = PROJECT_ABBREVIATIONS.get(proj_dir, proj_dir)
            dockerfile = os.path.join(os.path.dirname(csproj), 'Dockerfile')
            print_info(f"Options for {abbr}:")
            if csproj.endswith('.csproj'):
                print(f"{CYAN}1. Build{RESET}")
            if os.path.exists(dockerfile) or csproj.endswith('Dockerfile'):
                print(f"{CYAN}2. Create Docker image{RESET}")
                if csproj.endswith('.csproj'):
                    print(f"{CYAN}3. Start debug{RESET}")
            action = input(f"{YELLOW}Choose action (1/2/3, or 'b' to go back): {RESET}")
            if action == '1' and csproj.endswith('.csproj'):
                build_project(csproj)
            elif action == '2' and (os.path.exists(dockerfile) or csproj.endswith('Dockerfile')):
                docker_build(csproj)
            elif action == '3' and csproj.endswith('.csproj') and os.path.exists(dockerfile):
                start_debug(csproj)
            elif action.lower() == 'b':
                continue
            else:
                print_warning("Invalid action.")
            # After a command, return to main menu
            print_info("\nReturning to main menu...\n")
        except ValueError:
            print_warning("Invalid input.")

if __name__ == "__main__":
    main()
