# Requirements

## Functional Requirements

1. **FR1:** The application must accept user input for client gender (Male/Female), age (numeric input), and training days per week (1-7 days)

2. **FR2:** The system must generate personalized workout routines based on the combination of gender, age, and training frequency parameters

3. **FR3:** The application must maintain a local database of exercises with associated images and instructions

4. **FR4:** The system must export generated routines to Microsoft Word format (.docx) with embedded exercise images and formatted text

5. **FR5:** The application must load reference templates from a designated folder (PDFs/Word documents) to maintain consistent formatting

6. **FR6:** The interface must provide large, clearly labeled buttons and simple navigation suitable for users with minimal technical knowledge

7. **FR7:** The application must launch from a desktop shortcut/executable without requiring internet connectivity

8. **FR8:** The system must complete routine generation and export within 30 seconds of parameter selection

9. **FR9:** The application must display clear progress indicators during routine generation and export processes

10. **FR10:** The system must validate user inputs and provide clear error messages for invalid selections

## Non-Functional Requirements

1. **NFR1:** The application must start up in less than 5 seconds on Windows 10/11 systems

2. **NFR2:** El sistema debe funcionar completamente offline una vez instalado Ollama + Mistral 7B y la base de datos de ejercicios

14. **NFR14:** La aplicación debe requerir .NET 8 Runtime y funcionar nativamente en Windows 10 version 1809 o superior

15. **NFR15:** La aplicación debe utilizar WinUI 3 para una experiencia nativa de Windows con Fluent Design

3. **NFR3:** The user interface must be accessible to users aged 50+ with minimal technical experience

4. **NFR4:** The application must maintain 100% uptime during normal operation with zero crashes

5. **NFR5:** Generated Word documents must maintain professional formatting consistent with reference templates

6. **NFR6:** The application must require less than 15 minutes of learning time for basic users

7. **NFR7:** The system must use less than 500MB of disk space for core application and initial exercise database

8. **NFR8:** The application must support Windows 10 and Windows 11 operating systems
