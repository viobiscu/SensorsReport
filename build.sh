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
    echo ""
    read -p "Select action (1-6): " actionChoice
    
    case $actionChoice in
        1) selectedAction="build" ;;
        2) selectedAction="increment" ;;
        3) selectedAction="copy-flux" ;;
        4) selectedAction="docker-build" ;;
        5) selectedAction="build-and-push-docker" ;;
        6) selectedAction="apply-flux" ;;
        *) 
            echo "❌ Invalid selection!"
            exit 1
            ;;
    esac
}

# Function to execute the selected action
execute_action() {
    echo ""
    echo "=========================================="
    echo "Executing: $selectedAction for $selectedProjectName"
    echo "=========================================="
    
    # Handle all projects option
    if [[ "$selectedProject" == "ALL" ]]; then
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
