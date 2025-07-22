#!/bin/bash

# Interactive build script for SensorsReport projects
# Usage: ./build.sh

clear
echo "=========================================="
echo "  SensorsReport Interactive Build Tool"
echo "=========================================="

# Function to get all project directories
get_projects() {
    local excludedDirs=("flux" "krakend" "misc-files" "rest-client")
    local projects=()
    
    for dir in */; do
        if [[ -d "$dir" ]]; then
            local dirName=$(basename "$dir")
            local isExcluded=false
            
            # Check if directory is excluded
            for excluded in "${excludedDirs[@]}"; do
                if [[ "$dirName" == "$excluded" ]]; then
                    isExcluded=true
                    break
                fi
            done
            
            # Check if directory contains .csproj files
            if [[ "$isExcluded" == false ]] && ls "$dir"*.csproj 1> /dev/null 2>&1; then
                projects+=("$dir")
            fi
        fi
    done
    
    echo "${projects[@]}"
}

# Function to display project selection menu
select_project() {
    local projects=($(get_projects))
    
    if [[ ${#projects[@]} -eq 0 ]]; then
        echo "❌ No projects found!"
        exit 1
    fi
    
    echo ""
    echo "Available projects:"
    echo "==================="
    echo " 0) 🌟 All Projects"
    
    for i in "${!projects[@]}"; do
        local projectName=$(basename "${projects[$i]}" "/")
        printf "%2d) %s\n" $((i+1)) "$projectName"
    done
    
    echo ""
    read -p "Select project (0-${#projects[@]}): " projectChoice
    
    # Validate choice
    if [[ ! "$projectChoice" =~ ^[0-9]+$ ]] || [[ "$projectChoice" -lt 0 ]] || [[ "$projectChoice" -gt ${#projects[@]} ]]; then
        echo "❌ Invalid selection!"
        exit 1
    fi
    
    if [[ "$projectChoice" -eq 0 ]]; then
        selectedProject="ALL"
        selectedProjectName="All Projects"
        allProjects=($(get_projects))
    else
        selectedProject="${projects[$((projectChoice-1))]}"
        selectedProjectName=$(basename "$selectedProject" "/")
    fi
    echo "✅ Selected: $selectedProjectName"
}

# Function to display action selection menu
select_action() {
    echo ""
    echo "Available actions:"
    echo "=================="
    echo "1) Build project (dotnet build)"
    echo "2) Increment version (dotnet build -t:IncrementVersion)"
    echo "3) Copy flux files (dotnet build -t:CopyFluxFiles)"
    echo "4) Build Docker image (dotnet build -t:BuildDocker)"
    echo "5) Build and Push Docker (dotnet build -t:BuildAndPushDocker)"
    echo "6) Apply flux directory (kubectl apply -f flux/)"
    echo "7) Update Kustomization files"
    echo "8) Update Kustomization & Apply flux"
    echo ""
    read -p "Select action (1-8): " actionChoice
    
    case $actionChoice in
        1) selectedAction="build" ;;
        2) selectedAction="increment" ;;
        3) selectedAction="copy-flux" ;;
        4) selectedAction="docker-build" ;;
        5) selectedAction="build-and-push-docker" ;;
        6) selectedAction="apply-flux" ;;
        7) selectedAction="update-kustomization" ;;
        8) selectedAction="update-and-apply-flux" ;;
        *) 
            echo "❌ Invalid selection!"
            exit 1
            ;;
    esac
}

# Function to update kustomization files
update_kustomization() {
    echo "🔄 Updating kustomization files..."
    
    local flux_dir="flux"
    local main_kustomization="$flux_dir/kustomization.yaml"
    
    # Update main kustomization.yaml with direct file references
    echo "🔄 Updating main kustomization.yaml with direct file references..." >&2
    
    # Find existing service directories
    local service_dirs=($(find "$flux_dir" -maxdepth 1 -type d -name "sensors-report-*" | sort))
    
    # Create new kustomization.yaml content
    local kustomization_content=""
    kustomization_content+="apiVersion: kustomize.config.k8s.io/v1beta1"$'\n'
    kustomization_content+="kind: Kustomization"$'\n'
    kustomization_content+="namespace: default"$'\n'
    kustomization_content+="resources:"$'\n'
    
    # Add each YAML file from service directories
    for service_dir in "${service_dirs[@]}"; do
        local dir_name=$(basename "$service_dir")
        echo "📁 Processing directory: $dir_name" >&2
        
        # Find YAML files in directory (excluding kustomization files)
        local yaml_files=($(find "$service_dir" -maxdepth 1 -name "*.yaml" -o -name "*.yml" | grep -v kustomization | sort))
        
        if [[ ${#yaml_files[@]} -gt 0 ]]; then
            for yaml_file in "${yaml_files[@]}"; do
                local filename=$(basename "$yaml_file")
                kustomization_content+="  - $dir_name/$filename"$'\n'
                echo "  ✅ Added: $dir_name/$filename" >&2
            done
        else
            echo "  ⚠️  No YAML files found in $dir_name" >&2
        fi
    done
    
    # Write the content to file
    printf "%s" "$kustomization_content" > "$main_kustomization"
    
    echo "✅ Main kustomization.yaml updated with direct file references" >&2
    
    # Test kustomization if kustomize is available
    if command -v kustomize &> /dev/null; then
        echo "🧪 Testing kustomization..." >&2
        if kustomize build "$flux_dir" > /dev/null; then
            echo "✅ Kustomization test successful" >&2
        else
            echo "❌ Kustomization test failed" >&2
            echo "💡 Use 'git checkout $main_kustomization' to revert if needed" >&2
            return 1
        fi
    else
        echo "⚠️  Kustomize not installed, skipping test" >&2
    fi
    
    return 0
}

# Function to execute the selected action
execute_action() {
    echo ""
    echo "=========================================="
    echo "Executing: $selectedAction for $selectedProjectName"
    echo "=========================================="
    
    # Handle all projects option
    if [[ "$selectedProject" == "ALL" ]]; then
        # Special handling for kustomization actions
        if [[ "$selectedAction" == "update-kustomization" ]] || [[ "$selectedAction" == "update-and-apply-flux" ]]; then
            echo "🔄 Updating kustomization files for all services..."
            if update_kustomization; then
                echo "✅ Kustomization files updated successfully"
                if [[ "$selectedAction" == "update-and-apply-flux" ]]; then
                    echo "⚡ Applying flux directory..."
                    if kubectl apply -f flux/; then
                        echo "✅ Flux applied successfully"
                    else
                        echo "❌ Flux apply failed"
                        exit 1
                    fi
                fi
            else
                echo "❌ Kustomization update failed"
                exit 1
            fi
            return
        fi
        
        local successCount=0
        local failCount=0
        
        for project in "${allProjects[@]}"; do
            local projectName=$(basename "$project" "/")
            echo ""
            echo "🔄 Processing: $projectName"
            echo "----------------------------------------"
            
            cd "$project" || continue
            
            local executeCmd=""
            case $selectedAction in
                "build")
                    echo "🔨 Building project..."
                    executeCmd="dotnet build"
                    ;;
                "increment")
                    echo "📈 Incrementing version..."
                    executeCmd="dotnet build -t:IncrementVersion"
                    ;;
                "copy-flux")
                    echo "📁 Copying flux files..."
                    executeCmd="dotnet build -t:CopyFluxFiles"
                    ;;
                "docker-build")
                    if [[ ! -f "Dockerfile" ]]; then
                        echo "⚠️  Skipping (no Dockerfile found)"
                        cd ..
                        continue
                    fi
                    echo "🐳 Building Docker image..."
                    executeCmd="dotnet build -t:BuildDocker"
                    ;;
                "build-and-push-docker")
                    if [[ ! -f "Dockerfile" ]]; then
                        echo "⚠️  Skipping (no Dockerfile found)"
                        cd ..
                        continue
                    fi
                    echo "🚀 Building and pushing Docker image..."
                    executeCmd="dotnet build -t:BuildAndPushDocker"
                    ;;
                "apply-flux")
                    if [[ ! -d "flux" ]]; then
                        echo "⚠️  Skipping (no flux directory found)"
                        cd ..
                        continue
                    fi
                    echo "⚡ Applying flux directory..."
                    executeCmd="kubectl apply -f flux/"
                    ;;
            esac
            
            if eval "$executeCmd"; then
                echo "✅ $projectName completed successfully"
                ((successCount++))
            else
                echo "❌ $projectName failed"
                ((failCount++))
            fi
            
            cd ..
        done
        
        echo ""
        echo "=========================================="
        echo "📊 Summary: $successCount succeeded, $failCount failed"
        echo "=========================================="
    else
        # Handle single project
        cd "$selectedProject" || exit 1
    
        case $selectedAction in
            "build")
                echo "🔨 Building project..."
                dotnet build
                ;;
            "increment")
                echo "📈 Incrementing version..."
                dotnet build -t:IncrementVersion
                ;;
            "copy-flux")
                echo "📁 Copying flux files..."
                dotnet build -t:CopyFluxFiles
                ;;
            "docker-build")
                if [[ ! -f "Dockerfile" ]]; then
                    echo "❌ No Dockerfile found in project directory!"
                    exit 1
                fi
                echo "🐳 Building Docker image..."
                dotnet build -t:BuildDocker
                ;;
            "build-and-push-docker")
                if [[ ! -f "Dockerfile" ]]; then
                    echo "❌ No Dockerfile found in project directory!"
                    exit 1
                fi
                echo "🚀 Building and pushing Docker image..."
                dotnet build -t:BuildAndPushDocker
                ;;
            "apply-flux")
                if [[ ! -d "flux" ]]; then
                    echo "❌ No flux directory found in project directory!"
                    exit 1
                fi
                echo "⚡ Applying flux directory..."
                kubectl apply -f flux/
                ;;
            "update-kustomization")
                echo "🔄 Updating kustomization files..."
                cd ..
                update_kustomization
                cd "$selectedProject"
                ;;
            "update-and-apply-flux")
                echo "🔄 Updating kustomization and applying flux..."
                cd ..
                if update_kustomization; then
                    echo "⚡ Applying flux directory..."
                    kubectl apply -f flux/
                else
                    echo "❌ Kustomization update failed, skipping apply"
                    exit 1
                fi
                cd "$selectedProject"
                ;;
        esac
        
        if [[ $? -eq 0 ]]; then
            echo ""
            echo "✅ Action completed successfully!"
        else
            echo ""
            echo "❌ Action failed!"
            exit 1
        fi
    fi
}

# Main execution
select_project
select_action
execute_action

echo ""
echo "=========================================="
echo "Build process completed!"
echo "=========================================="
