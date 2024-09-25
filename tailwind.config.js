import daisyui from 'daisyui'

export default {
    content: [
        "./src/CollabGateway.Client/.fable-build/**/*.{js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {
            animation: {
                fade: 'fadeIn 500ms ease-in-out',
            },
            keyframes: {
                fadeIn: {
                    '0%': { opacity: '0' },
                    '100%': { opacity: '1' },
                },
            },
        },
    },
    plugins: [
        daisyui,
    ],
    daisyui: {
        themes: ["dark", "fantasy"], // false: only light + dark | true: all themes | array: specific themes like this ["light", "dark", "cupcake"]
        base: true, // applies background color and foreground color for root element by default
        styled: true, // include daisyUI colors and design decisions for all components
        utils: true, // adds responsive and modifier utility classes
        prefix: "", // prefix for daisyUI classnames (components, modifiers and responsive class names. Not colors)
        logs: true, // Shows info about daisyUI version and used config in the console when building your CSS
        themeRoot: ":root", // The element that receives theme color CSS
    },
}