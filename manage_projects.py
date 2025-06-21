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
                explorer_dir = os.path.join(root, "Sensors-Report-Explorer")
                version_file = os.path.join(explorer_dir, "version.txt")
                dockerfile = os.path.join(explorer_dir, "Dockerfile.backend")
                print_info("Options for Explorer Backend:")
                print(f"{CYAN}1. Apply Deployment{RESET}")
                print(f"{CYAN}2. Apply Service{RESET}")
                print(f"{CYAN}3. Get Pod Status{RESET}")
                print(f"{CYAN}4. Get Logs{RESET}")
                print(f"{CYAN}5. Build, Tag, and Push Backend Docker Image (auto-increment version){RESET}")
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
                    label = "sensors-report-explorer-backend"
                    print_info(f"Getting pod status for {label} ...")
                    subprocess.run(["kubectl", "get", "pods", "-l", f"app={label}"])
                elif action == '4':
                    label = "sensors-report-explorer-backend"
                    print_info(f"Getting logs for {label} ...")
                    pods = subprocess.check_output(["kubectl", "get", "pods", "-l", f"app={label}", "-o", "jsonpath={.items[0].metadata.name}"]).decode().strip()
                    if pods:
                        subprocess.run(["kubectl", "logs", pods])
                    else:
                        print_error(f"No pod found for {label}")
                elif action == '5':
                    # Build, tag, and push backend image with versioning
                    # Increment version
                    version = None
                    if os.path.exists(version_file):
                        with open(version_file) as vf:
                            current = vf.read().strip()
                        if current.isdigit():
                            version = str(int(current) + 1)
                        else:
                            version = '1'
                    else:
                        version = '1'
                    with open(version_file, 'w') as vf:
                        vf.write(version)
                    image_name = f"viobiscu/sensors-report-explorer-backend:{version}"
                    print_info(f"Building Docker image {image_name} ...")
                    result = subprocess.run(["docker", "build", "-f", dockerfile, "-t", image_name, explorer_dir])
                    if result.returncode == 0:
                        print_success(f"Docker build succeeded for {image_name}.")
                        print_info(f"Pushing Docker image {image_name} to repository ...")
                        push_result = subprocess.run(["docker", "push", image_name])
                        if push_result.returncode == 0:
                            print_success(f"Docker image {image_name} pushed successfully.")
                            # Update deployment YAML with new image and annotation
                            if os.path.exists(deployment_yaml):
                                print_info(f"Updating deployment YAML {deployment_yaml} with new image and version ...")
                                import yaml as pyyaml
                                with open(deployment_yaml, 'r') as f:
                                    deployment = pyyaml.safe_load(f)
                                containers = deployment['spec']['template']['spec']['containers']
                                containers[0]['image'] = image_name
                                # Update annotation to trigger rollout
                                annotations = deployment['spec']['template']['metadata'].get('annotations', {})
                                annotations['backend-image-version'] = str(version)
                                deployment['spec']['template']['metadata']['annotations'] = annotations
                                with open(deployment_yaml, 'w') as f:
                                    pyyaml.safe_dump(deployment, f)
                                print_info(f"Applying updated deployment YAML {deployment_yaml} ...")
                                subprocess.run(["kubectl", "apply", "-f", deployment_yaml])
                                print_success("Kubernetes backend deployment updated with new image version!")
                            else:
                                print_error(f"Deployment YAML not found: {deployment_yaml}")
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
            elif proj_dir == "Sensors-Report-Explorer-Frontend":
                frontend_yaml_dir = os.path.join(root, "flux", "sensors-report-explorer")
                deployment_yaml = os.path.join(frontend_yaml_dir, "frontend-deployment.yaml")
                service_yaml = os.path.join(frontend_yaml_dir, "frontend-service.yaml")
                explorer_dir = os.path.join(root, "Sensors-Report-Explorer")
                version_file = os.path.join(explorer_dir, "version.txt")
                set_version_script = os.path.join(explorer_dir, "set-frontend-version.sh")
                print_info("Options for Explorer Frontend:")
                print(f"{CYAN}1. Apply Deployment{RESET}")
                print(f"{CYAN}2. Apply Service{RESET}")
                print(f"{CYAN}3. Get Pod Status{RESET}")
                print(f"{CYAN}4. Get Logs{RESET}")
                print(f"{CYAN}5. Build, Tag, and Push Frontend Docker Image (auto-increment version){RESET}")
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
                    label = "sensors-report-explorer-frontend"
                    print_info(f"Getting pod status for {label} ...")
                    subprocess.run(["kubectl", "get", "pods", "-l", f"app={label}"])
                elif action == '4':
                    label = "sensors-report-explorer-frontend"
                    print_info(f"Getting logs for {label} ...")
                    pods = subprocess.check_output(["kubectl", "get", "pods", "-l", f"app={label}", "-o", "jsonpath={.items[0].metadata.name}"]).decode().strip()
                    if pods:
                        subprocess.run(["kubectl", "logs", pods])
                    else:
                        print_error(f"No pod found for {label}")
                elif action == '5':
                    # Build, tag, and push frontend image with versioning
                    if not os.path.exists(set_version_script):
                        print_error(f"set-frontend-version.sh not found in {explorer_dir}")
                    else:
                        print_info("Incrementing version, building, and pushing frontend Docker image ...")
                        # Run the script from the correct directory
                        result = subprocess.run(["bash", "set-frontend-version.sh"], cwd=explorer_dir)
                        if result.returncode == 0:
                            print_success("Frontend Docker image built and tagged with new version.")
                            # Optionally, push the image
                            version = None
                            if os.path.exists(version_file):
                                with open(version_file) as vf:
                                    version = vf.read().strip()
                            image_name = f"viobiscu/sensors-report-explorer-frontend:{version}" if version else "viobiscu/sensors-report-explorer-frontend:latest"
                            print_info(f"Pushing Docker image {image_name} to repository ...")
                            push_result = subprocess.run(["docker", "push", image_name])
                            if push_result.returncode == 0:
                                print_success(f"Docker image {image_name} pushed successfully.")
                                print_info(f"Update your deployment YAML to use image: {image_name} and env FRONTEND_IMAGE_VERSION: {version}")
                                # Automatically update and apply the deployment YAML
                                if os.path.exists(deployment_yaml):
                                    print_info(f"Updating deployment YAML {deployment_yaml} with new image and version ...")
                                    # Read YAML, update image and env, write back
                                    import yaml as pyyaml
                                    with open(deployment_yaml, 'r') as f:
                                        deployment = pyyaml.safe_load(f)
                                    containers = deployment['spec']['template']['spec']['containers']
                                    containers[0]['image'] = image_name
                                    # Update env var
                                    env_list = containers[0].get('env', [])
                                    found = False
                                    for env in env_list:
                                        if env['name'] == 'FRONTEND_IMAGE_VERSION':
                                            env['value'] = str(version)
                                            found = True
                                            break
                                    if not found:
                                        env_list.append({'name': 'FRONTEND_IMAGE_VERSION', 'value': str(version)})
                                    containers[0]['env'] = env_list
                                    # Update annotation to trigger rollout
                                    annotations = deployment['spec']['template']['metadata'].get('annotations', {})
                                    annotations['frontend-image-version'] = str(version)
                                    deployment['spec']['template']['metadata']['annotations'] = annotations
                                    with open(deployment_yaml, 'w') as f:
                                        pyyaml.safe_dump(deployment, f)
                                    print_info(f"Applying updated deployment YAML {deployment_yaml} ...")
                                    subprocess.run(["kubectl", "apply", "-f", deployment_yaml])
                                    print_success("Kubernetes deployment updated with new image version!")
                                else:
                                    print_error(f"Deployment YAML not found: {deployment_yaml}")
                            else:
                                print_error(f"Failed to push Docker image {image_name}.")
                        else:
                            print_error("Failed to build/tag frontend Docker image.")
                elif action.lower() == 'b':
                    continue
                else:
                    print_warning("Invalid action.")
                print_info("\nReturning to main menu...\n")
                continue
            # Apply unified menu for other backend APIs
            if proj_dir in [
                "Sensors-Report-Audit.API",
                "Sensors-Report-Business-Broker.API",
                "Sensors-Report-Audit",
                "Sensors-Report-MQTT-to-Orion",
                "Sensors-Report-Provision.API",
                "Sensors-Report-Email.API",
                "Sensors-Report-SMS.API",
                "Sensors-Report-Workflow.API"
            ]:
                # Determine paths
                project_dir = os.path.join(root, proj_dir)
                deployment_yaml = os.path.join(root, "flux", proj_dir.replace('.', '-').lower(), "deployment-test.yaml")
                service_yaml = os.path.join(root, "flux", proj_dir.replace('.', '-').lower(), "service.yaml")
                version_file = os.path.join(project_dir, "version.txt")
                dockerfile = os.path.join(project_dir, "Dockerfile")
                print_info(f"Options for {menu_name}:")
                print(f"{CYAN}1. Apply Deployment{RESET}")
                print(f"{CYAN}2. Apply Service{RESET}")
                print(f"{CYAN}3. Get Pod Status{RESET}")
                print(f"{CYAN}4. Get Logs{RESET}")
                print(f"{CYAN}5. Build, Tag, and Push Docker Image (auto-increment version){RESET}")
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
                    label = proj_dir.replace('.', '-').lower()
                    print_info(f"Getting pod status for {label} ...")
                    subprocess.run(["kubectl", "get", "pods", "-l", f"app={label}"])
                elif action == '4':
                    label = proj_dir.replace('.', '-').lower()
                    print_info(f"Getting logs for {label} ...")
                    try:
                        pods = subprocess.check_output([
                            "kubectl", "get", "pods", "-l", f"app={label}", "-o", "jsonpath={.items[0].metadata.name}"]
                        ).decode().strip()
                        if pods:
                            subprocess.run(["kubectl", "logs", pods])
                        else:
                            print_error(f"No pod found for {label}")
                    except Exception as e:
                        print_error(f"Error getting logs: {e}")
                elif action == '5':
                    # Auto-increment version
                    version = None
                    if os.path.exists(version_file):
                        with open(version_file) as vf:
                            current = vf.read().strip()
                        if current.isdigit():
                            version = str(int(current) + 1)
                        else:
                            version = '1'
                    else:
                        version = '1'
                    with open(version_file, 'w') as vf:
                        vf.write(version)
                    image_name = f"viobiscu/{proj_dir.lower().replace('.', '-')}-backend:{version}"
                    print_info(f"Building Docker image {image_name} ...")
                    result = subprocess.run(["docker", "build", "-f", dockerfile, "-t", image_name, project_dir])
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
    # Define csproj before usage
    csproj = "path/to/your/project.csproj"  # Replace with actual logic to determine the .csproj file
    main()
