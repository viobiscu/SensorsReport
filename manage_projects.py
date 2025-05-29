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
    "Sensors-Report-Explorer": "Explorer",
    "Sensors-Report-Provision.API": "Provision.API",
    "Sensors-Report-Workflow.API": "Workflow.API"
}

# Docker image names for each project
DOCKER_IMAGE_NAMES = {
    "Sensors-Report-Audit": "viobiscu/sensors-report-audit:latest",
    "Sensors-Report-Audit.API": "viobiscu/sensors-report-audit:latest",
    "Sensors-Report-Business-Broker.API": "viobiscu/sensors-report-business-broker:latest",
    "Sensors-Report-MQTT-to-Orion": "viobiscu/sensors-report-mqtt-to-orion:latest",
    "Sensors-Report-Explorer": "viobiscu/sensors-report-explorer:latest",
    "Sensors-Report-Provision.API": "viobiscu/sensors-report-provision-api:latest",
    "Sensors-Report-Workflow.API": "viobiscu/sensors-report-workflow-api:latest"
}

# Add new project abbreviations and docker image names for frontend/backend
PROJECT_ABBREVIATIONS.update({
    "Sensors-Report-Explorer-Backend": "Explorer.Backend",
    "Sensors-Report-Explorer-Frontend": "Explorer.Frontend"
})
DOCKER_IMAGE_NAMES.update({
    "Sensors-Report-Explorer-Backend": "viobiscu/sensors-report-explorer:latest",
    "Sensors-Report-Explorer-Frontend": "nginx:alpine"
})

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
    import os
    print_info(f"Updating Flux manifest {manifest_path} with image {new_image} ...")
    manifest_path = os.path.expanduser(manifest_path)
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

def docker_build(csproj, skip_menu=False, only_push=False, only_k8s=False, only_flux=False):
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
            if skip_menu:
                return
            if only_push:
                # Push image to repo
                print_info(f"Pushing Docker image {image_name} to repository ...")
                result = subprocess.run(["docker", "push", image_name])
                if result.returncode == 0:
                    print_success("Docker image pushed successfully.")
                else:
                    print_error("Failed to push Docker image.")
                return
            if only_k8s:
                # Update local k8s
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
                    print_warning(f"Could not find *-deployment.yaml for this project in flux directory. Looked in: {flux_dir} and subfolders for files matching '*-deployment.yaml' and project name '{proj_dir.replace('.', '-').lower()}'.")
                if service_yaml:
                    print_info(f"Applying {service_yaml} ...")
                    subprocess.run(["kubectl", "apply", "-f", service_yaml])
                else:
                    print_warning(f"Could not find *-service.yaml for this project in flux directory. Looked in: {flux_dir} and subfolders for files matching '*-service.yaml' and project name '{proj_dir.replace('.', '-').lower()}'.")
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
            elif only_flux:
                # Update production via Flux (interactive, persistent path)
                config_file = os.path.join(os.path.dirname(os.path.abspath(__file__)), ".flux_manifest_path")
                last_path = None
                if os.path.exists(config_file):
                    with open(config_file, "r") as f:
                        last_path = f.read().strip()
                while True:
                    prompt = f"{CYAN}Enter path to flux deployment manifest"
                    if last_path:
                        prompt += f" [{last_path}]"
                    prompt += f": {RESET}"
                    manifest_path = input(prompt).strip()
                    if not manifest_path and last_path:
                        manifest_path = last_path
                    manifest_path_expanded = os.path.expanduser(manifest_path)
                    if os.path.isfile(manifest_path_expanded):
                        # Save for next time
                        with open(config_file, "w") as f:
                            f.write(manifest_path)
                        break
                    else:
                        print_error(f"File not found: {manifest_path_expanded}. Please enter a valid path.")
                update_flux_manifest(manifest_path, image_name)
                return
            # ...no menu here, handled by deployment submenu...
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

    # Custom menu order and names
    MENU_PROJECTS = [
        ("Audit.API", "Sensors-Report-Audit.API"),
        ("Business-Broker.API", "Sensors-Report-Business-Broker.API"),
        ("Audit", "Sensors-Report-Audit"),
        ("MQTT-to-Orion", "Sensors-Report-MQTT-to-Orion"),
        ("Provision.API", "Sensors-Report-Provision.API"),
        ("Email.API", "Sensors-Report-Email.API"),
        ("SMS.API", "Sensors-Report-SMS.API"),
        ("Explorer Backend", "Sensors-Report-Explorer-Backend"),
        ("Explorer Frontend", "Sensors-Report-Explorer-Frontend"),
        ("Workflow.API", "Sensors-Report-Workflow.API")
    ]
    while True:
        for idx, (menu_name, proj_dir) in enumerate(MENU_PROJECTS):
            print(f"{CYAN}{idx+1}. {menu_name}{RESET}")
        print(f"{CYAN}Q. Quit Program{RESET}")
        choice = input(f"{YELLOW}Select a project by number (or 'Q' to quit): {RESET}")
        if choice.lower() == 'q':
            break
        try:
            idx = int(choice) - 1
            if idx < 0 or idx >= len(MENU_PROJECTS):
                print_warning("Invalid selection.")
                continue
            menu_name, proj_dir = MENU_PROJECTS[idx]
            # Special handling for Explorer Backend/Frontend
            if proj_dir == "Sensors-Report-Explorer-Backend":
                backend_yaml_dir = os.path.join(root, "flux", "sensors-report-explorer")
                deployment_yaml = os.path.join(backend_yaml_dir, "backend-deployment.yaml")
                service_yaml = os.path.join(backend_yaml_dir, "backend-service.yaml")
                dockerfile = os.path.join(root, "Sensors-Report-Explorer", "Dockerfile")
                csproj = dockerfile  # Use Dockerfile as project marker
                print_info("Options for Explorer Backend:")
            elif proj_dir == "Sensors-Report-Explorer-Frontend":
                frontend_yaml_dir = os.path.join(root, "flux", "sensors-report-explorer")
                deployment_yaml = os.path.join(frontend_yaml_dir, "frontend-deployment.yaml")
                service_yaml = os.path.join(frontend_yaml_dir, "frontend-service.yaml")
                dockerfile = None  # Nginx image, no Dockerfile
                csproj = None
                print_info("Options for Explorer Frontend:")
            else:
                # Find the project file or Dockerfile for this menu entry
                csproj = None
                for proj in projects:
                    if proj_dir in proj:
                        csproj = proj
                        break
                if not csproj:
                    print_error(f"Project files for {menu_name} not found.")
                    continue
                dockerfile = os.path.join(os.path.dirname(csproj), 'Dockerfile')
                deployment_yaml = None
                service_yaml = None
            # Menu for backend/frontend
            if proj_dir.startswith("Sensors-Report-Explorer-"):
                print(f"{CYAN}1. Apply Deployment{RESET}")
                print(f"{CYAN}2. Apply Service{RESET}")
                print(f"{CYAN}3. Get Pod Status{RESET}")
                print(f"{CYAN}4. Get Logs{RESET}")
                print(f"{CYAN}5. Build and Push Docker Image{RESET}")
                print(f"{CYAN}B. Back to Main Menu{RESET}")
                action = input(f"{YELLOW}Choose action (1-5, or 'B' to go back): {RESET}")
                if action == '1':
                    if os.path.exists(deployment_yaml):
                        print_info(f"Applying {deployment_yaml} ...")
                        subprocess.run(["kubectl", "apply", "-f", deployment_yaml])
                    else:
                        print_error(f"Deployment YAML not found: {deployment_yaml}")
                elif action == '2':
                    if os.path.exists(service_yaml):
                        print_info(f"Applying {service_yaml} ...")
                        subprocess.run(["kubectl", "apply", "-f", service_yaml])
                    else:
                        print_error(f"Service YAML not found: {service_yaml}")
                elif action == '3':
                    label = "sensors-report-explorer-backend" if proj_dir.endswith("Backend") else "sensors-report-explorer-frontend"
                    print_info(f"Getting pod status for {label} ...")
                    subprocess.run(["kubectl", "get", "pods", "-l", f"app={label}"])
                elif action == '4':
                    label = "sensors-report-explorer-backend" if proj_dir.endswith("Backend") else "sensors-report-explorer-frontend"
                    print_info(f"Getting logs for {label} ...")
                    pods = subprocess.check_output(["kubectl", "get", "pods", "-l", f"app={label}", "-o", "jsonpath={.items[0].metadata.name}"]).decode().strip()
                    if pods:
                        subprocess.run(["kubectl", "logs", pods])
                    else:
                        print_error(f"No pod found for {label}")
                elif action == '5':
                    # Build and push Docker image for backend/frontend
                    if proj_dir.endswith("Backend"):
                        dockerfile = os.path.join(root, "Sensors-Report-Explorer", "Dockerfile.backend")
                        image_name = "viobiscu/sensors-report-explorer-backend:latest"
                    else:
                        dockerfile = os.path.join(root, "Sensors-Report-Explorer", "Dockerfile.frontend")
                        image_name = "viobiscu/sensors-report-explorer-frontend:latest"
                    print_info(f"Building Docker image {image_name} ...")
                    result = subprocess.run(["docker", "build", "-f", dockerfile, "-t", image_name, os.path.join(root, "Sensors-Report-Explorer")])
                    if result.returncode == 0:
                        print_success(f"Docker build succeeded for {image_name}.")
                        print_info(f"Pushing Docker image {image_name} to repository ...")
                        push_result = subprocess.run(["docker", "push", image_name])
                        if push_result.returncode == 0:
                            print_success(f"Docker image {image_name} pushed successfully.")
                        else:
                            print_error(f"Failed to push Docker image {image_name}.")
                    else:
                        print_error(f"Docker build failed for {image_name}.")
                elif action.lower() == 'b':
                    continue
                else:
                    print_warning("Invalid action.")
                print_info("\nReturning to main menu...\n")
                continue
            # ...existing code for other projects...
            print_info(f"Options for {menu_name}:")
            if csproj.endswith('.csproj') or (os.path.exists(dockerfile) or csproj.endswith('Dockerfile')):
                print(f"{CYAN}1. Build Debug{RESET}")
                print(f"{CYAN}2. Build Production{RESET}")
                print(f"{CYAN}3. Start Debug{RESET}")
                print(f"{CYAN}4. Start Production{RESET}")
                print(f"{CYAN}5. Deployment{RESET}")
                print(f"{CYAN}B. Back to Main Menu{RESET}")
                action = input(f"{YELLOW}Choose action (1-5, or 'B' to go back): {RESET}")
                if action == '1':
                    print_info("Building Debug...")
                    result = subprocess.run(['dotnet', 'build', csproj, '--configuration', 'Debug'])
                    if result.returncode == 0:
                        print_success("Build Debug succeeded.")
                    else:
                        print_error("Build Debug failed.")
                elif action == '2':
                    print_info("Cleaning previous build artifacts...")
                    clean_result = subprocess.run(['dotnet', 'clean', csproj, '--configuration', 'Release'])
                    if clean_result.returncode == 0:
                        print_success("Clean succeeded.")
                    else:
                        print_error("Clean failed.")
                    # Read and display version
                    version_file = os.path.join(os.path.dirname(csproj), 'version.txt')
                    version = None
                    if os.path.exists(version_file):
                        with open(version_file) as vf:
                            version = vf.read().strip()
                        print_info(f"Build Version: {version}")
                    else:
                        print_warning("version.txt not found!")
                    print_info("Building Production (Release)...")
                    build_result = subprocess.run(['dotnet', 'build', csproj, '--configuration', 'Release'])
                    if build_result.returncode == 0:
                        print_success("Build Production succeeded.")
                        # Optionally, verify version in output (if needed, add logic here)
                    else:
                        print_error("Build Production failed.")
                elif action == '3':
                    print_info("Starting Debug...")
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
                elif action == '4':
                    print_info("Starting Production (Release)...")
                    try:
                        proc = subprocess.Popen(['dotnet', 'run', '--project', csproj, '--configuration', 'Release'])
                        input(f"{YELLOW}Press Enter to stop and return to the menu...\n{RESET}")
                        proc.terminate()
                        proc.wait()
                        print_success("Production run stopped.")
                    except KeyboardInterrupt:
                        print_warning("\nProduction run stopped by user.")
                    except Exception as e:
                        print_error(f"Error during production run: {e}")
                elif action == '5':
                    # Deployment submenu
                    while True:
                        print(f"{CYAN}Options for deployment:{RESET}")
                        print(f"{CYAN}1. Create Docker image{RESET}")
                        print(f"{CYAN}2. Push image to repo{RESET}")
                        print(f"{CYAN}3. Update local k8s{RESET}")
                        print(f"{CYAN}4. Remove from local k8s{RESET}")
                        print(f"{CYAN}5. Update production via Flux{RESET}")
                        print(f"{CYAN}b. Back to menu.{RESET}")
                        dep_action = input(f"{YELLOW}Select a deployment option: {RESET}")
                        if dep_action == '1':
                            docker_build(csproj, skip_menu=True)
                        elif dep_action == '2':
                            docker_build(csproj, only_push=True)
                        elif dep_action == '3':
                            docker_build(csproj, only_k8s=True)
                        elif dep_action == '4':
                            # Remove from local k8s
                            workspace_root = os.path.dirname(os.path.abspath(__file__))
                            flux_dir = os.path.join(workspace_root, 'flux')
                            proj_dir = os.path.basename(os.path.dirname(csproj))
                            deployment_yaml = None
                            for subdir in os.listdir(flux_dir):
                                subdir_path = os.path.join(flux_dir, subdir)
                                if os.path.isdir(subdir_path):
                                    candidate_deploy = os.path.join(subdir_path, 'deployment-test.yaml')
                                    if os.path.exists(candidate_deploy) and proj_dir.replace('.', '-').lower() in candidate_deploy:
                                        deployment_yaml = candidate_deploy
                            if deployment_yaml:
                                import yaml as pyyaml
                                with open(deployment_yaml, 'r') as f:
                                    deployment = pyyaml.safe_load(f)
                                deployment_name = deployment['metadata']['name']
                                print_info(f"Removing deployment {deployment_name} from local k8s ...")
                                subprocess.run(["kubectl", "delete", "deployment", deployment_name])
                            else:
                                print_warning(f"Could not find deployment-test.yaml for this project in flux directory. Looked in: {flux_dir} and subfolders for files matching 'deployment-test.yaml' and project name '{proj_dir.replace('.', '-').lower()}'.")
                        elif dep_action == '5':
                            docker_build(csproj, only_flux=True)
                        elif dep_action.lower() == 'b':
                            break
                        else:
                            print_warning("Invalid action.")
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
