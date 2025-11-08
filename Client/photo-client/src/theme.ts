import { createTheme } from '@mui/material/styles'

// Light brand palette to match mock
const brandBg = '#f3ede6' // page background
const brandPaper = '#ffffff' // cards
const brandAccent = '#5a3a0d' // dark brown accents
const brandAccentHover = '#7a5015'
const brandText = '#2b1d12' // primary text
const brandMuted = '#705e4d'

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: brandAccent, contrastText: '#ffffff' },
    secondary: { main: brandAccentHover },
    background: { default: brandBg, paper: brandPaper },
    text: { primary: brandText, secondary: brandMuted }
  },
  typography: {
    fontFamily: 'Inter, ui-sans-serif, system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", Arial'
  },
  components: {
    MuiAppBar: {
      styleOverrides: {
        root: {
          backgroundColor: brandAccent,
          color: '#fff'
        }
      }
    },
    MuiButton: {
      styleOverrides: {
        root: { textTransform: 'none', fontWeight: 600, letterSpacing: '.2px' },
        containedPrimary: {
          backgroundColor: brandAccent,
          '&:hover': { backgroundColor: brandAccentHover }
        }
      }
    },
    MuiCard: {
      styleOverrides: {
        root: {
          background: brandPaper,
          border: '1px solid #e2d8cf'
        }
      }
    }
  }
})

export default theme
