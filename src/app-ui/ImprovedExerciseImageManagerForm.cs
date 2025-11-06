using System;
using System.Windows.Forms;

namespace GymRoutineGenerator.UI
{
    [Obsolete("ImprovedExerciseImageManagerForm ha sido eliminado. Usa el gestor actual: HybridExerciseManagerForm.")]
    public partial class ImprovedExerciseImageManagerForm : Form
    {
        public ImprovedExerciseImageManagerForm()
        {
            // Mensaje informativo para desarrolladores que accidentalmente instancien este formulario
            MessageBox.Show("El formulario 'ImprovedExerciseImageManagerForm' es obsoleto y ha sido retirado. Usa el gestor actual (HybridExerciseManagerForm).",
                "Formulario obsoleto", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Cerrar inmediatamente para no mostrar UI antigua
            this.Load += (s, e) => this.Close();
        }
    }
}
