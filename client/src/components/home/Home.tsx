import { useAuth } from '../../context/AuthProvider';
import { useBoard } from '../../context/BoardProvider';
import AppTheme from "../shared-theme/AppTheme.tsx";
import AccountCircleIcon from "@mui/icons-material/AccountCircle";
import ViewColumnIcon from "@mui/icons-material/ViewColumn";
import Box from "@mui/material/Box";
import CssBaseline from "@mui/material/CssBaseline";
import IconButton from "@mui/material/IconButton";
import Link from "@mui/material/Link";
import Stack from "@mui/material/Stack";
import {Popover} from "@mui/material";
import * as React from "react";
import Divider from "@mui/material/Divider";
import {Outlet} from "react-router";

export default function Home(props: { disableCustomTheme?: boolean }) {
  const { logout } = useAuth();
  const { boards, GetAllBoards } = useBoard();
  const [anchorEl, setAnchorEl] = React.useState<HTMLButtonElement | null>(null);
  const [boardsLoaded, setBoardsLoaded] = React.useState(false);
  const hasLoadedBoards = React.useRef(false);

  const handleAccountClick = (event: React.MouseEvent<HTMLButtonElement>) => {
      setAnchorEl(event.currentTarget);
  }
  const handleAccountClose = () => {
      setAnchorEl(null);
  }
  const accountOpen = Boolean(anchorEl);
  const accountId = accountOpen ? 'simple-popover' : undefined;

  React.useEffect(() => {
      if (hasLoadedBoards.current) {
          return;
      }
      hasLoadedBoards.current = true;
      void (async () => {
          try {
              await GetAllBoards();
          } finally {
              setBoardsLoaded(true);
          }
      })();
  }, [GetAllBoards]);

  return (
    <AppTheme {...props}>
        <CssBaseline enableColorScheme />
        <Stack sx={{ minHeight: '100vh' }}>
            <Box sx={{ }}
            >
                <Box
                    sx={{
                        position: 'relative',
                        display: 'flex',
                        alignItems: 'center',
                        width: '100%',
                        justifyContent: 'center',
                        height: '48px'
                    }}
                >
                    <IconButton type="button" sx={{ position: 'absolute', top: 0, right: 0, border: 'none' }} aria-label="account" onClick={handleAccountClick}>
                        <AccountCircleIcon />
                    </IconButton>
                    <Popover
                        id={accountId}
                        open={accountOpen}
                        anchorEl={anchorEl}
                        onClose={handleAccountClose}
                        anchorOrigin={{
                            vertical: 'bottom',
                            horizontal: 'right',
                        }}
                        >
                        <Link
                            component="button"
                            type="button"
                            onClick={logout}
                            variant="body2"
                            sx={{ p: 2, display: 'block' }}
                        >
                            Logout
                        </Link>
                    </Popover>
                </Box>
            </Box>
            <Divider />
            <Box sx={{ display: 'flex', width: '100%', flex: 1, minHeight: 0 }}>
                <Box sx={{ width: 288, paddingLeft: '40px' }}>
                    <Stack spacing={5} sx={{ paddingTop: '50px' }}>
                        <Link href="/boards" underline="hover" sx={{ display: 'inline-flex', alignItems: 'center', gap: 1.5 }}>
                            <ViewColumnIcon fontSize="small" />
                            Boards
                        </Link>
                    </Stack>
                </Box>
                <Divider orientation={'vertical'} flexItem />
                <Outlet context={{ boards, boardsLoaded }} />
            </Box>
        </Stack>
    </AppTheme>
  );
}
