import daisyui from 'daisyui'

export default {
    content: [
        "./src/CollabGateway.Client/.fable-build/**/*.{js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {
            animation: {
                fade: 'fadeIn 500ms ease-in-out',
                fadeOut: 'fadeOut 1s ease-in-out',
            },
            keyframes: {
                fadeIn: {
                    '0%': { opacity: '0' },
                    '100%': { opacity: '1' },
                },
                fadeOut: {
                    '0%': { opacity: '1' },
                    '100%': { opacity: '0' },
                },
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
                    ...require("daisyui/src/theming/themes")["business"],
                    accent: "#ED5B00",
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