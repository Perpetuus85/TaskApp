import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import { Card, CardActionArea } from '@mui/material';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import * as React from 'react';
import { useOutletContext } from 'react-router';
import { useBoard } from '../../context/BoardProvider';

type BoardItem = {
  id: number | string;
  name: string;
};

type BoardsOutletContext = {
  boards: BoardItem[];
  boardsLoaded: boolean;
};

export default function BoardList() {
  const { GetAllBoards, CreateBoard } = useBoard();
  const { boards, boardsLoaded } = useOutletContext<BoardsOutletContext>();
  const [createDialogOpen, setCreateDialogOpen] = React.useState(false);
  const [boardName, setBoardName] = React.useState('');
  const [isCreatingBoard, setIsCreatingBoard] = React.useState(false);
  const [createBoardError, setCreateBoardError] = React.useState('');

  const handleCreateOpen = () => {
    setCreateBoardError('');
    setCreateDialogOpen(true);
  };

  const handleCreateClose = () => {
    if (isCreatingBoard) {
      return;
    }
    setCreateDialogOpen(false);
    setCreateBoardError('');
    setBoardName('');
  };

  const handleCreateSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const trimmedName = boardName.trim();
    if (!trimmedName) {
      return;
    }

    setCreateBoardError('');
    setIsCreatingBoard(true);
    try {
      await CreateBoard({ name: trimmedName });
      await GetAllBoards();
      setCreateDialogOpen(false);
      setBoardName('');
    } catch {
      setCreateBoardError('Something went wrong. Please try again.');
    } finally {
      setIsCreatingBoard(false);
    }
  };

  return (
    <Box sx={{ flex: 1, paddingLeft: '48px' }}>
      <Box
        sx={{
          display: 'block',
          maxWidth: '1000px',
          marginTop: '24px',
          marginLeft: 'auto',
          marginRight: 'auto',
          marginBottom: '0',
        }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 2 }}>
          <Typography
            component="h1"
            variant="h3"
            sx={{ width: '100%', fontSize: 'clamp(1.25rem, 10vw, 1.5rem)', textAlign: 'left' }}
          >
            Your Boards
          </Typography>
          <Button type="button" variant="contained" onClick={handleCreateOpen}>
            +
          </Button>
        </Box>
        {boardsLoaded && boards.length === 0 && (
          <Typography variant="body1" sx={{ mt: 1, textAlign: 'left' }}>
            You currently have no boards.
          </Typography>
        )}
        {boards.length > 0 && (
          <Box sx={{ mt: 3, display: 'flex', flexWrap: 'wrap', gap: 2 }}>
            {boards.map((board) => (
              <Card key={board.id} variant="outlined" sx={{ padding: '0', width: '100%', maxWidth: '23%', height: '108px' }}>
                <CardActionArea href={`/boards/${board.id}`}>
                  <Box component="div" sx={{ height: '70px', backgroundColor: '#669DF1' }} />
                  <Typography variant="h6" component="div">
                    {board.name}
                  </Typography>
                </CardActionArea>
              </Card>
            ))}
          </Box>
        )}
      </Box>
      <Dialog open={createDialogOpen} onClose={handleCreateClose} fullWidth maxWidth="sm">
        <Box component="form" onSubmit={handleCreateSubmit}>
          <DialogTitle>Create New Board</DialogTitle>
          <DialogContent>
            <TextField
              autoFocus
              fullWidth
              placeholder="Name"
              value={boardName}
              onChange={(event) => setBoardName(event.target.value)}
              margin="dense"
            />
            {createBoardError && (
              <Typography variant="body2" color="error" sx={{ mt: 1 }}>
                {createBoardError}
              </Typography>
            )}
          </DialogContent>
          <DialogActions sx={{ paddingRight: '20px' }}>
            <Button type="button" onClick={handleCreateClose} disabled={isCreatingBoard}>
              Cancel
            </Button>
            <Button type="submit" disabled={isCreatingBoard || boardName.trim().length === 0}>
              Save
            </Button>
          </DialogActions>
        </Box>
      </Dialog>
    </Box>
  );
}
