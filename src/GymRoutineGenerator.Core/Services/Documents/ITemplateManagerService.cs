using System.Threading.Tasks;
using System.Collections.Generic;

namespace GymRoutineGenerator.Core.Services.Documents;

public interface ITemplateManagerService
{
    Task<List<DocumentTemplate>> GetAvailableTemplatesAsync();
    Task<DocumentTemplate> GetTemplateAsync(string templateId);
    Task<DocumentTemplate> SaveCustomTemplateAsync(DocumentTemplate template);
    Task<bool> DeleteCustomTemplateAsync(string templateId);
    DocumentTemplate CreateTemplateFromBase(TemplateType baseType, string name, string description = "");
    Task<DocumentPreview> PreviewTemplateAsync(DocumentTemplate template, RoutineDocumentRequest sampleRequest);
}