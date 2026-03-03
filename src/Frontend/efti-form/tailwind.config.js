/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        brand: {
          50:  '#f0effe',
          100: '#dddcfc',
          200: '#bbb8f8',
          400: '#7B6FD0',
          500: '#5F54C0',
          600: '#3D3580',
          700: '#2e2860',
        },
        cyan: {
          400: '#00C2CB',
          500: '#00adb5',
        },
      },
    },
  },
  plugins: [],
}
