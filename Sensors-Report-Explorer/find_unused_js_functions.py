import os
import re

WORKSPACE_ROOT = '.'  # Change to your workspace root if needed
JS_FOLDER = 'js'      # Folder to scan for JS files

def find_js_files(root):
    js_files = []
    for dirpath, _, filenames in os.walk(root):
        for f in filenames:
            if f.endswith('.js'):
                js_files.append(os.path.join(dirpath, f))
    return js_files

def extract_functions(js_file):
    with open(js_file, 'r', encoding='utf-8') as f:
        content = f.read()
    # Match function foo(, export function foo(, const foo = (, let foo = (, var foo = (
    pattern = re.compile(r'(?:export\s+)?function\s+([a-zA-Z0-9_]+)\s*\(|(?:const|let|var)\s+([a-zA-Z0-9_]+)\s*=\s*\(')
    matches = pattern.findall(content)
    # Flatten and filter empty
    return [m[0] or m[1] for m in matches if m[0] or m[1]]

def is_function_used(function_name, workspace_files, defining_file):
    pattern = re.compile(r'\b' + re.escape(function_name) + r'\b')
    for file in workspace_files:
        if file == defining_file:
            continue
        with open(file, 'r', encoding='utf-8', errors='ignore') as f:
            if pattern.search(f.read()):
                return True
    return False

def main():
    js_folder_path = os.path.join(WORKSPACE_ROOT, JS_FOLDER)
    js_files = find_js_files(js_folder_path)
    workspace_files = []
    for dirpath, _, filenames in os.walk(WORKSPACE_ROOT):
        for f in filenames:
            if f.endswith(('.js', '.html', '.htm', '.ts', '.jsx', '.tsx')):
                workspace_files.append(os.path.join(dirpath, f))

    unused_functions = []

    for js_file in js_files:
        functions = extract_functions(js_file)
        for func in functions:
            if not is_function_used(func, workspace_files, js_file):
                unused_functions.append((func, js_file))

    if unused_functions:
        print("Unused (possibly orphaned) functions:")
        for func, js_file in unused_functions:
            print(f"  {func}  (in {js_file})")
    else:
        print("No unused functions found.")

if __name__ == '__main__':
    main()
