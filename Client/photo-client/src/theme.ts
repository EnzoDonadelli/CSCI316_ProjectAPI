import { createTheme } from '@mui/material/styles'

const darkRed = '#7a0b0b'
const almostBlack = '#0b0b0b'
const nearWhite = '#f7f7f7'

const theme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: darkRed,
      contrastText: nearWhite
    },
    background: {
      default: almostBlack,
      paper: '#121212'
    },
    text: {
      primary: nearWhite,
      secondary: '#cccccc'
    }
  },
  components: {
    MuiButton: {
      styleOverrides: {
        containedPrimary: {
          backgroundColor: darkRed,
          color: nearWhite
        }
      }
    }
  }
})

export default theme
