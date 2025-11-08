using System.Drawing;

namespace GymRoutineGenerator.UI
{
    /// <summary>
    /// Paleta de colores premium para la aplicación - Tema lujoso
    /// Negro, Blanco, Dorado, Violeta Premium, Verde Oscuro Premium
    /// </summary>
    public static class PremiumColors
    {
        // Colores Base
        public static readonly Color Black = Color.FromArgb(10, 10, 10);           // Negro suave
        public static readonly Color PureBlack = Color.FromArgb(0, 0, 0);          // Negro puro
        public static readonly Color White = Color.FromArgb(255, 255, 255);        // Blanco puro
        public static readonly Color OffWhite = Color.FromArgb(250, 250, 250);     // Blanco suave

        // Dorados (Gold)
        public static readonly Color Gold = Color.FromArgb(212, 175, 55);          // Dorado clásico
        public static readonly Color GoldLight = Color.FromArgb(255, 215, 0);      // Dorado brillante
        public static readonly Color GoldDark = Color.FromArgb(184, 134, 11);      // Dorado oscuro
        public static readonly Color GoldAccent = Color.FromArgb(201, 176, 55);    // Dorado acento

        // Violetas Premium
        public static readonly Color VioletDark = Color.FromArgb(75, 0, 130);      // Violeta oscuro (Indigo)
        public static readonly Color Violet = Color.FromArgb(106, 13, 173);        // Violeta premium
        public static readonly Color VioletBright = Color.FromArgb(138, 43, 226);  // Violeta brillante
        public static readonly Color VioletLight = Color.FromArgb(147, 51, 234);   // Violeta claro

        // Verdes Oscuros Premium
        public static readonly Color GreenDark = Color.FromArgb(11, 70, 25);       // Verde bosque oscuro
        public static readonly Color GreenEmerald = Color.FromArgb(27, 94, 32);    // Verde esmeralda
        public static readonly Color GreenForest = Color.FromArgb(46, 125, 50);    // Verde bosque
        public static readonly Color GreenAccent = Color.FromArgb(34, 139, 34);    // Verde acento

        // Colores de Fondo
        public static readonly Color BackgroundDark = Color.FromArgb(18, 18, 18);  // Fondo oscuro
        public static readonly Color BackgroundCard = Color.FromArgb(28, 28, 30);  // Fondo tarjetas oscuro
        public static readonly Color BackgroundLight = Color.FromArgb(245, 245, 245); // Fondo claro alternativo

        // Colores de Texto
        public static readonly Color TextPrimary = White;
        public static readonly Color TextSecondary = Color.FromArgb(200, 200, 200);
        public static readonly Color TextOnLight = Black;
        public static readonly Color TextGold = Gold;

        // Colores de Bordes
        public static readonly Color BorderGold = Color.FromArgb(150, 212, 175, 55);     // Borde dorado semi-transparente
        public static readonly Color BorderViolet = Color.FromArgb(100, 106, 13, 173);   // Borde violeta semi-transparente
        public static readonly Color BorderDark = Color.FromArgb(50, 50, 50);

        // Sombras
        public static readonly Color ShadowDark = Color.FromArgb(40, 0, 0, 0);
        public static readonly Color ShadowGold = Color.FromArgb(30, 212, 175, 55);

        // Botones Premium
        public static class Buttons
        {
            // Botón Principal (Dorado)
            public static readonly Color PrimaryNormal = GoldDark;
            public static readonly Color PrimaryHover = Gold;
            public static readonly Color PrimaryPressed = Color.FromArgb(150, 100, 0);

            // Botón Secundario (Violeta)
            public static readonly Color SecondaryNormal = VioletDark;
            public static readonly Color SecondaryHover = Violet;
            public static readonly Color SecondaryPressed = Color.FromArgb(50, 0, 100);

            // Botón Éxito (Verde)
            public static readonly Color SuccessNormal = GreenDark;
            public static readonly Color SuccessHover = GreenEmerald;
            public static readonly Color SuccessPressed = Color.FromArgb(8, 50, 18);

            // Botón Acento (Dorado Brillante)
            public static readonly Color AccentNormal = GoldAccent;
            public static readonly Color AccentHover = GoldLight;
            public static readonly Color AccentPressed = GoldDark;
        }

        // Tarjetas Premium
        public static class Cards
        {
            public static readonly Color Background = BackgroundCard;
            public static readonly Color BackgroundLight = Color.FromArgb(35, 35, 38);
            public static readonly Color Border = BorderGold;
            public static readonly Color Shadow = ShadowDark;
            public static readonly Color TitleGold = Gold;
            public static readonly Color TitleViolet = VioletBright;
        }

        // Estados
        public static class States
        {
            public static readonly Color Success = GreenForest;
            public static readonly Color Warning = GoldLight;
            public static readonly Color Error = Color.FromArgb(139, 0, 0);  // Rojo oscuro
            public static readonly Color Info = VioletBright;
            public static readonly Color Disabled = Color.FromArgb(80, 80, 80);
        }
    }
}
