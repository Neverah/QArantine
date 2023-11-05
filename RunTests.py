import argparse
import os
import subprocess

# Prints an error in red
def print_error(text):
    print('\033[91m' + text + '\033[0m')

# Runs a specific test and waits for it to end
def exec_test(proj_root_dir, test_name):
    print(f'Running test: {test_name}')
    command = f'dotnet run --project "{proj_root_dir}" -- {test_name}'
    process = subprocess.Popen(command, shell=True)
    process.wait()

# For common functions when all tests have been executed
def on_tests_execution_end():
    print('Tests execution finished')

# MAIN
def main():
    # Parse console arguments
    parser = argparse.ArgumentParser(description = 'Script to run tests.')
    parser.add_argument('-l', '--lib', help='Force to run only tests within a test library with the given name')
    parser.add_argument('-t', '--test', help='Force to run only a test with the given name (must define --lib too)')
    parser.add_argument('-trd', '--tests-root-dir', help='Specifies the root directory of the tests, if it is not the default one')
    parser.add_argument('-prd', '--proj-root-dir', help='Specifies the root directory of the project, if it is not the default one')

    args = parser.parse_args()

    test_library_name = args.lib if args.lib else "*"
    test_name = args.test if args.test else "*"
    tests_root_dir = args.tests_root_dir if args.tests_root_dir else ".\\FrameworkTests"
    proj_root_dir = args.proj_root_dir if args.proj_root_dir else "."

    # Check if only a specific library or test has been requested to be tested and, if so, that they are valid
    if test_library_name != "*":
        if not os.path.exists(os.path.join(tests_root_dir, test_library_name)):
            print_error(f'[ERROR] The selected test library directory does not exist: "{os.path.join(tests_root_dir, test_library_name)}"')
            return
    
        if test_name != "*":
            if not os.path.exists(os.path.join(tests_root_dir, test_library_name, test_name)):
                print_error(f'[ERROR] The selected test directory does not exist: "{os.path.join(tests_root_dir, test_library_name, test_name)}"')
                return
            else:
                # If only one test and library are specified, it is executed and the script will be terminated after that
                exec_test(proj_root_dir, test_name)
                on_tests_execution_end()
                return

    # If not specified up to the specific test, all tests that meet the filters are run 
    for _,libs,_ in os.walk(tests_root_dir):
        for lib_dir in libs:
            if test_library_name == "*" or lib_dir == test_library_name:
                print(f'Found test library: {lib_dir}')
                for _,tests,_ in os.walk(os.path.join(tests_root_dir, lib_dir)):
                    for test in tests:
                        print(f'Found test: {test}')
                        exec_test(proj_root_dir, test)
                        
    on_tests_execution_end()

# This ensures that the main() is only executed if this is the main script, not if it is imported from another script
if __name__ == "__main__":
    main()
