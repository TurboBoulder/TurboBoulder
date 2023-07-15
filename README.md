# Turbo Boulder
Turbo Boulder is an open-source project dedicated to accelerating your .NET web application development process. It offers a comprehensive boilerplate for .NET applications, including API support, local user handling, and self-signed certificates for development.

The platform leverages Docker containers to provide a standardized and replicable development environment, encompassing all components, including databases. This streamlined approach simplifies the setup process, enabling developers to focus more on building their applications rather than dealing with configuration.

Turbo Boulder supports easy installation through Linux's 'apt-get' or Windows' MSI, ensuring accessibility for developers on various operating systems. As an open-source project, Turbo Boulder thrives on community collaboration and welcomes contributions from developers worldwide.

Turbo Boulder is currently not usable, but should reach a point where it can be tested before the end of july.

## Features

### CLI Provisioning Tool
- Cross-platform support: Runs on both Windows and Linux systems, with respective precompiled versions.
- Interactive setup: Guides the user through the setup process, automating Docker container setup and configuration.

### Dotnet Core API
- .NET Core framework: Efficient backend operations facilitated by the .NET Core framework.
- Modular design: Adherence to API best practices for a modular and scalable architecture.
- Security measures: Implementation of modern security practices to protect data and interactions.

### Blazor Frontend
- Minimalistic design: Provides a simple, minimalistic interface as a starting point for customization.
- Flexibility: Offers users the freedom to apply their own vision and design preferences.
- Cross-platform: Web-based design ensures compatibility across different platforms.

### MS SQL Database
- Reliable storage: Utilizes MS SQL for robust and secure data storage.
- Integration: Seamless integration with the Dotnet Core API for efficient data operations.
- Scalability: Scales to accommodate growing data needs without compromising performance.

### Management Interface
- Configuration management: Allows for straightforward configuration changes during the development phase.
- Deployment process: Provides an easy way to initiate the deployment process.

### Docker Containers
- Isolation: Each application service runs in its own Docker container, minimizing conflicts.
- Consistency: Containerization ensures consistent application behavior across different environments.
- Version-controlled: Docker container configurations are tracked and controlled within the project's GitHub repository.

### GitHub Integration
- Version control: GitHub is used for version control, tracking changes to the codebase and Docker container configurations.
- Docker management: The GitHub Docker Registry is used for convenient Docker container management.

...
## Usage

### Installation

#### Linux

1. Use `apt-get` to install the CLI tool: 
    ```bash
    sudo apt-get install <cli-tool-name>
    ```

#### Windows

1. Download the MSI package and follow the installation wizard steps.

### Setup

1. Navigate to the directory where you want to create the project.
2. Run the CLI tool in your console. If you already have a name for your project, you can include it as a parameter:
    ```bash
    turboboulder <project-name>
    ```
    If you do not include a project name, the tool will prompt you to enter one. 

    A new subdirectory with your project name will be created, and the tool will guide you through the setup process, configuring the necessary Docker containers.

### Accessing Project Files

After the setup, you can access the project files with your preferred code editor:

- The API project files are located in the `api` subdirectory within your project folder.
- The frontend project files are located in the `frontend` subdirectory within your project folder.

## Contributing

Hello! Thanks for your interest in contributing to this project! We value your effort and time and are always excited to see community participation. We believe that every contribution, be it in the form of code, bug reports, or feature requests, can significantly enhance the project and benefit all users.

### Newbie Alert
I'd like to highlight that I am quite new to managing a GitHub project. Therefore, my turnaround time for reviewing and integrating contributions, especially pull requests, might not be as speedy as you might expect from seasoned project maintainers. I kindly ask for your patience and understanding as I familiarize myself with the process.

### Guidelines
Before making any contributions, please take a moment to review our Contributing Guidelines (if available). This document outlines the standards and procedures that we ask contributors to adhere to.

Please keep in mind that as I navigate the ropes of project maintenance, I might inadvertently overlook something or be slow to respond. Your patience and cooperation are highly appreciated.

### Pull Requests
If you'd like to contribute code, the process is as follows:

- Fork the repo.
- Create a new branch in your fork.
- Make your changes in the new branch.
- Submit a pull request from your branch to the main repo.
- I will try to review pull requests as quickly as I can, but please bear with me as I may take a bit longer to fully understand and evaluate your changes.

### Issues
If you're looking to report a bug or request a feature, please use the Issues tab on GitHub. Be as specific as possible in your report to help me understand the problem or feature request.

### Code of Conduct
We strive to foster an open and welcoming environment. Please follow our Code of Conduct (if available) when interacting with others within the project.

Thank you again for considering contributing to this project! Every bit of help makes a difference, and your patience and understanding are greatly appreciated as I learn the ropes of this exciting journey.

## License
This project is licensed under the terms of the MIT license.

For the full license text, please see the [LICENSE](LICENSE) file in the project root.

