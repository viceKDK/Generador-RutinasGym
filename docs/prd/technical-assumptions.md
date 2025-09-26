# Technical Assumptions

## Repository Structure: Monorepo
Single repository containing frontend, backend services, database schemas, and asset management for streamlined development by a single developer.

## Service Architecture
**Native Windows Desktop Application with Local AI** with the following components:
- **Frontend:** WinUI 3 for native Windows desktop experience with modern UI
- **Backend:** .NET 8 application with integrated business logic and Ollama communication
- **Database:** SQLite with Entity Framework Core for local exercise database and user preferences
- **AI Integration:** Ollama + Mistral 7B via HTTP API calls for intelligent routine generation
- **Document Generation:** Open XML SDK or DocumentFormat.OpenXml for Word document creation with image embedding

## Testing Requirements
**Unit + Integration Testing with MSTest** focusing on:
- Core routine generation algorithm testing with MSTest attributes
- Word document generation verification using Assert methods
- Ollama integration testing with model availability checks and mocking
- UI component testing for accessibility compliance
- Entity Framework Core database operations testing

## Additional Technical Assumptions and Requests

**Frontend Framework:**
- **WinUI 3:** Native Windows app with modern Fluent Design System
- **XAML:** Declarative UI with data binding and MVVM pattern
- **Large button/input components:** Custom XAML controls optimized for accessibility and grandmother-friendly interface

**Backend Services:**
- **.NET 8:** Integrated application architecture with business logic
- **HttpClient:** For communicating with Ollama REST API (localhost:11434)
- **Entity Framework Core + SQLite:** Modern ORM with lightweight database for desktop application

**Document Generation:**
- **DocumentFormat.OpenXml:** Official Microsoft library for Word document creation
- **System.Drawing or ImageSharp:** Image processing for exercise photos optimization
- **Template system:** Custom C# classes for dynamic document generation based on user preferences

**AI Integration - Local Model:**
- **Ollama + Mistral 7B:** Local AI model running on user's machine for intelligent routine generation
- **Model Management:** Automatic Ollama installation and model download during app setup
- **Prompt engineering:** Structured prompts in Spanish/English for fitness routine generation
- **Model Flexibility:** Easy switching between different Ollama models (Llama, CodeLlama, etc.)
- **Fallback algorithm:** Simple rule-based system if Ollama fails or model unavailable

**Development Tools:**
- **JetBrains Rider:** Primary IDE with excellent WinUI 3 support and debugging
- **MSTest:** Testing framework for unit and integration tests
- **Inno Setup or MSIX:** Windows installer creation
- **.NET CLI:** Build and package management
