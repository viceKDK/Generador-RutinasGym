/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#D4AF37', // Gold Premium
          dark: '#B8860B',
          light: '#FFD700',
          50: '#FFFBEB',
          100: '#FEF3C7',
          200: '#FDE68A',
          300: '#FCD34D',
          400: '#FBBF24',
          500: '#D4AF37',
          600: '#B8860B',
          700: '#92400E',
          800: '#78350F',
          900: '#451A03',
        },
        secondary: {
          DEFAULT: '#6A0DAD', // Violet Premium
          dark: '#4B0082',
          light: '#8A2BE2',
          50: '#FAF5FF',
          100: '#F3E8FF',
          200: '#E9D5FF',
          300: '#D8B4FE',
          400: '#C084FC',
          500: '#A855F7',
          600: '#9333EA',
          700: '#7E22CE',
          800: '#6B21A8',
          900: '#581C87',
        },
        accent: {
          DEFAULT: '#C9B037',
          emerald: '#059669',
          jade: '#10B981',
        },
        background: {
          DEFAULT: '#0A0A0A',
          card: '#121212',
          elevated: '#1A1A1A',
        },
        surface: {
          DEFAULT: '#1C1C1E',
          light: '#252525',
          lighter: '#2F2F2F',
        },
        text: {
          DEFAULT: '#FFFFFF',
          primary: '#FFFFFF',
          secondary: '#E5E5E5',
          muted: '#A0A0A0',
        },
        border: {
          DEFAULT: '#404040',
          light: '#505050',
          gold: 'rgba(212, 175, 55, 0.3)',
          violet: 'rgba(106, 13, 173, 0.3)',
        },
        success: '#10B981',
        warning: '#F59E0B',
        error: '#EF4444',
      },
      fontFamily: {
        sans: ['-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'Oxygen', 'Ubuntu', 'Cantarell', 'sans-serif'],
        display: ['Inter', 'system-ui', 'sans-serif'],
      },
      animation: {
        'fade-in': 'fadeIn 0.5s cubic-bezier(0.4, 0, 0.2, 1)',
        'fade-in-up': 'fadeInUp 0.6s cubic-bezier(0.4, 0, 0.2, 1)',
        'slide-in': 'slideIn 0.5s cubic-bezier(0.4, 0, 0.2, 1)',
        'slide-in-right': 'slideInRight 0.5s cubic-bezier(0.4, 0, 0.2, 1)',
        'scale-in': 'scaleIn 0.4s cubic-bezier(0.4, 0, 0.2, 1)',
        'shimmer': 'shimmer 2s linear infinite',
        'glow': 'glow 2s ease-in-out infinite alternate',
        'float': 'float 3s ease-in-out infinite',
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0', transform: 'translateY(10px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        fadeInUp: {
          '0%': { opacity: '0', transform: 'translateY(30px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        slideIn: {
          '0%': { transform: 'translateX(-100%)', opacity: '0' },
          '100%': { transform: 'translateX(0)', opacity: '1' },
        },
        slideInRight: {
          '0%': { transform: 'translateX(100%)', opacity: '0' },
          '100%': { transform: 'translateX(0)', opacity: '1' },
        },
        scaleIn: {
          '0%': { opacity: '0', transform: 'scale(0.95)' },
          '100%': { opacity: '1', transform: 'scale(1)' },
        },
        shimmer: {
          '0%': { backgroundPosition: '-1000px 0' },
          '100%': { backgroundPosition: '1000px 0' },
        },
        glow: {
          '0%': { boxShadow: '0 0 20px rgba(212, 175, 55, 0.2)' },
          '100%': { boxShadow: '0 0 40px rgba(212, 175, 55, 0.6), 0 0 60px rgba(106, 13, 173, 0.3)' },
        },
        float: {
          '0%, 100%': { transform: 'translateY(0px)' },
          '50%': { transform: 'translateY(-10px)' },
        },
      },
      boxShadow: {
        'premium': '0 10px 40px rgba(212, 175, 55, 0.1), 0 2px 8px rgba(0, 0, 0, 0.2)',
        'premium-lg': '0 20px 60px rgba(212, 175, 55, 0.15), 0 8px 16px rgba(0, 0, 0, 0.3)',
        'glow-gold': '0 0 20px rgba(212, 175, 55, 0.4), 0 0 40px rgba(212, 175, 55, 0.2)',
        'glow-violet': '0 0 20px rgba(106, 13, 173, 0.4), 0 0 40px rgba(106, 13, 173, 0.2)',
        'inner-glow': 'inset 0 2px 8px rgba(212, 175, 55, 0.1)',
      },
      backgroundImage: {
        'gradient-radial': 'radial-gradient(var(--tw-gradient-stops))',
        'gradient-premium': 'linear-gradient(135deg, #D4AF37 0%, #6A0DAD 100%)',
        'gradient-gold': 'linear-gradient(135deg, #FFD700 0%, #B8860B 100%)',
        'gradient-violet': 'linear-gradient(135deg, #8A2BE2 0%, #4B0082 100%)',
        'shimmer': 'linear-gradient(90deg, transparent, rgba(255,255,255,0.1), transparent)',
      },
    },
  },
  plugins: [],
}
