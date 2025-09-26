# Epic 1: Foundation & Local AI Setup

**Epic Goal:** Establish the foundational project infrastructure including Electron application framework, Ollama integration with Mistral 7B, basic SQLite database, and initial UI scaffold. By the end of this epic, the application can generate a simple "Hello World" workout routine demonstrating the complete end-to-end flow.

## Story 1.1: Project Setup & Electron Framework
As a developer,
I want to establish the basic Electron + React project structure,
so that I have a foundation for building the desktop application.

### Acceptance Criteria
1. Electron application launches with basic React UI
2. Project structure includes frontend, backend, and database directories
3. Development environment configured with hot reload
4. Basic packaging script creates executable
5. Application displays "GymRoutine Generator" title and placeholder content

## Story 1.2: SQLite Database Foundation
As a developer,
I want to establish SQLite database with basic exercise schema,
so that the application can store and retrieve workout data.

### Acceptance Criteria
1. SQLite database file created in user's data directory
2. Basic exercise table with columns: id, name, muscle_group, equipment, image_path
3. Database connection module working from Node.js backend
4. Seed data with 10-15 basic exercises inserted
5. Simple API endpoint returns exercise list

## Story 1.3: Ollama Installation & Model Setup
As a user,
I want the application to automatically set up Ollama with Mistral 7B,
so that I can use AI-powered routine generation without manual configuration.

### Acceptance Criteria
1. Application checks if Ollama is installed on system startup
2. Automatic Ollama installation process (Windows) with user permission
3. Mistral 7B model download with progress indicator
4. Health check endpoint verifies Ollama + model are working
5. Clear error messages if installation fails with manual instructions

## Story 1.4: Basic AI Integration Test
As a developer,
I want to establish communication with local Mistral 7B model,
so that I can verify AI integration works before building complex features.

### Acceptance Criteria
1. Simple prompt sent to Ollama returns structured response
2. Basic workout generation prompt tested (hardcoded parameters)
3. Response parsing extracts exercise recommendations
4. Fallback mechanism activated if Ollama unavailable
5. Basic error handling for AI communication failures

## Story 1.5: Hello World Routine Generator
As a user,
I want to generate a basic workout routine with minimal input,
so that I can verify the core application functionality works.

### Acceptance Criteria
1. Simple UI form with gender and training days inputs
2. "Generate Routine" button triggers AI-powered generation
3. Generated routine displays as text list on screen
4. Progress indicator shown during generation process
5. Success/error states clearly communicated to user
