import daisyui from 'daisyui'

export default {
    content: [
        "./src/CollabGateway.Client/.fable-build/**/*.{js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {
            colors: {
                'accent-light': '#d2be68'
            },
            animation: {
                'fadeIn': 'fadeIn 500ms ease-in-out',
                'fadeOut': 'fadeOut 500s ease-in-out',
                'pulse-ring': 'pulse-ring 1s cubic-bezier(0.4, 0, 0.6, 1) infinite',
                'pulse-button': 'pulse-button 1s cubic-bezier(0.4, 0, 0.6, 1) infinite'
            },
            keyframes: {
                'fadeIn': {
                    '0%': { opacity: '0' },
                    '100%': { opacity: '1' },
                },
                'fadeOut': {
                    '0%': { opacity: '1' },
                    '100%': { opacity: '0' },
                },
                'pulse-ring': {
                    '0%, 100%': {
                        borderColor: 'theme("colors.accent")',
                        boxShadow: '0 0 0 1px theme("colors.accent")'
                    },
                    '50%': {
                        borderColor: 'theme("colors.accent-light")',
                        boxShadow: '0 0 0 2px theme("colors.accent-light")'
                    },
                },
                'pulse-button': {
                    '0%, 100%': {
                        outline: '2px solid theme("colors.accent")',
                        outlineOffset: '1px'
                    },
                    '50%': {
                        outline: '3px solid theme("colors.accent-light")',
                        outlineOffset: '2px'
                    }
                }
            },
        },
    },
    plugins: [
        daisyui,
    ],
    daisyui: {
        themes: [
            {
                business: {
                    "primary": "#1C4E80",
                    "secondary": "#7C909A",
                    "accent": "#ED5B00",  // Your custom accent
                    "neutral": "#23282E",
                    "base-100": "#010d12",  // Your custom background
                    "info": "#0EA5E9",
                    "success": "#22C55E",
                    "warning": "#F59E0B",
                    "error": "#EF4444",
                },
            },
            {
                nord: {
                    ...require("daisyui/src/theming/themes")["nord"],
                    accent: "#ED5B00",
                },
            }
        ],
        base: true,
        styled: true,
        utils: true,
        prefix: "",
        logs: true,
        themeRoot: ":root",
    },
}