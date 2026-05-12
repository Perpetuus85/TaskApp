import * as React from 'react';
import Typography from '@mui/material/Typography';
import { useParams } from 'react-router';
import { useBoard, type Board as BoardModel } from '../../context/BoardProvider';
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Divider from "@mui/material/Divider";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import MenuItem from "@mui/material/MenuItem";
import TextField from "@mui/material/TextField";
import IconButton from "@mui/material/IconButton";
import {DatePicker, LocalizationProvider} from "@mui/x-date-pickers";
import {AdapterDayjs} from "@mui/x-date-pickers/AdapterDayjs";
import dayjs, {Dayjs} from "dayjs";
import Task from "../tasks/Task";
import utc from "dayjs/plugin/utc";
import EditIcon from '@mui/icons-material/Edit';

export default function Board() {
  const { GetBoardWithTasks, CreateBoardTask, UpdateBoardTask, DeleteBoardTask, UpdateBoard } = useBoard();
  const { boardId } = useParams();
  const [board, setBoard] = React.useState<BoardModel | null>(null);
  const [createTaskOpen, setCreateTaskOpen] = React.useState(false);
  const [updateTaskOpen, setUpdateTaskOpen] = React.useState(false);
  const [taskSummary, setTaskSummary] = React.useState('');
  const [taskDescription, setTaskDescription] = React.useState('');
  const [taskDueDate, setTaskDueDate] = React.useState<Dayjs | null>(null);
  const [taskStatus, setTaskStatus] = React.useState('ToDo');
  const [updateTaskId, setUpdateTaskId] = React.useState('');
  const [updateTaskSummary, setUpdateTaskSummary] = React.useState('');
  const [updateTaskDescription, setUpdateTaskDescription] = React.useState('');
  const [updateTaskDueDate, setUpdateTaskDueDate] = React.useState<Dayjs | null>(null);
  const [updateTaskStatus, setUpdateTaskStatus] = React.useState('ToDo');
  const [isCreatingTask, setIsCreatingTask] = React.useState(false);
  const [isUpdatingTask, setIsUpdatingTask] = React.useState(false);
  const [updateBoardOpen, setUpdateBoardOpen] = React.useState(false);
  const [updateBoardName, setUpdateBoardName] = React.useState('');
  const [isUpdatingBoard, setIsUpdatingBoard] = React.useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = React.useState(false);
  const [isDeletingTask, setIsDeletingTask] = React.useState(false);
  const [createTaskError, setCreateTaskError] = React.useState('');
  const [updateTaskError, setUpdateTaskError] = React.useState('');
  const [updateBoardError, setUpdateBoardError] = React.useState('');
  dayjs.extend(utc);

  React.useEffect(() => {
    if (!boardId) {
      return;
    }
    void (async () => {
      const boardResponse = await GetBoardWithTasks(boardId);
      setBoard(boardResponse as BoardModel);
    })();
  }, [GetBoardWithTasks, boardId]);

  const handleOpenCreateTask = () => {
    setCreateTaskError('');
    setCreateTaskOpen(true);
  };

  const handleOpenUpdateBoard = () => {
    setUpdateBoardError('');
    setUpdateBoardName(board?.name ?? '');
    setUpdateBoardOpen(true);
  };

  const handleCloseUpdateBoard = () => {
    if (isUpdatingBoard) {
      return;
    }
    setUpdateBoardOpen(false);
    setUpdateBoardError('');
    setUpdateBoardName('');
  };

  const handleCloseCreateTask = () => {
    if (isCreatingTask) {
      return;
    }
    setCreateTaskOpen(false);
    setCreateTaskError('');
    setTaskSummary('');
    setTaskDescription('');
    setTaskDueDate(null);
    setTaskStatus('ToDo');
  };

  const handleOpenUpdateTask = (task: { id: string; summary: string; description?: string; dueAt?: string; status?: string }) => {
    setUpdateTaskId(task.id);
    setUpdateTaskSummary(task.summary ?? '');
    setUpdateTaskDescription(task.description ?? '');
    setUpdateTaskDueDate(task.dueAt ? dayjs(task.dueAt) : null);
    setUpdateTaskStatus(task.status === 'ToDo' ? 'ToDo' : (task.status ?? 'ToDo'));
    setUpdateTaskError('');
    setUpdateTaskOpen(true);
  };

  const handleCloseUpdateTask = () => {
    if (isUpdatingTask || isDeletingTask) {
      return;
    }
    setUpdateTaskOpen(false);
    setUpdateTaskError('');
    setUpdateTaskId('');
    setUpdateTaskSummary('');
    setUpdateTaskDescription('');
    setUpdateTaskDueDate(null);
    setUpdateTaskStatus('ToDo');
  };

  const handleOpenDeleteConfirm = () => {
    setDeleteConfirmOpen(true);
  };

  const handleCloseDeleteConfirm = () => {
    if (isDeletingTask) {
      return;
    }
    setDeleteConfirmOpen(false);
  };

  const handleCreateTaskSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const trimmedSummary = taskSummary.trim();
    if (!boardId || !trimmedSummary) {
      return;
    }

    setCreateTaskError('');
    setIsCreatingTask(true);
    try {
      await CreateBoardTask({
        boardId,
        summary: trimmedSummary,
        description: taskDescription.trim(),
        dueDate: taskDueDate?.utc().format('YYYY-MM-DDTHH:mm:ss[Z]'),
        status: taskStatus,
      });
      const updatedBoard = await GetBoardWithTasks(boardId);
      setBoard(updatedBoard as BoardModel);
      setCreateTaskOpen(false);
      setTaskSummary('');
      setTaskDescription('');
      setTaskDueDate(null);
      setTaskStatus('To Do');
    } catch {
      setCreateTaskError('Something went wrong. Please try again.');
    } finally {
      setIsCreatingTask(false);
    }
  };

  const handleUpdateTaskSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const trimmedSummary = updateTaskSummary.trim();
    if (!boardId || !updateTaskId || !trimmedSummary) {
      return;
    }

    setUpdateTaskError('');
    setIsUpdatingTask(true);
    try {
      await UpdateBoardTask({
        id: updateTaskId,
        summary: trimmedSummary,
        description: updateTaskDescription.trim(),
        dueDate: updateTaskDueDate?.utc().format('YYYY-MM-DDTHH:mm:ss[Z]'),
        status: updateTaskStatus,
      });
      const updatedBoard = await GetBoardWithTasks(boardId);
      setBoard(updatedBoard as BoardModel);
      handleCloseUpdateTask();
    } catch {
      setUpdateTaskError('Something went wrong. Please try again.');
    } finally {
      setIsUpdatingTask(false);
    }
  };

  const handleDeleteTaskConfirm = async () => {
    if (!boardId || !updateTaskId) {
      return;
    }

    setIsDeletingTask(true);
    try {
      await DeleteBoardTask({ id: updateTaskId });
      const updatedBoard = await GetBoardWithTasks(boardId);
      setBoard(updatedBoard as BoardModel);
      setDeleteConfirmOpen(false);
      handleCloseUpdateTask();
    } finally {
      setIsDeletingTask(false);
    }
  };

  const handleUpdateBoardSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const trimmedName = updateBoardName.trim();
    if (!boardId || !trimmedName) {
      return;
    }

    setUpdateBoardError('');
    setIsUpdatingBoard(true);
    try {
      await UpdateBoard({
        id: boardId,
        name: trimmedName,
      });
      const updatedBoard = await GetBoardWithTasks(boardId);
      setBoard(updatedBoard as BoardModel);
      handleCloseUpdateBoard();
    } catch {
      setUpdateBoardError('Something went wrong. Please try again.');
    } finally {
      setIsUpdatingBoard(false);
    }
  };

  const boardTasks = (board?.boardTasks ?? []) as Array<{
    id: string;
    summary: string;
    description?: string;
    dueAt?: string;
    status?: string;
  }>;
  const toDoTasks = boardTasks.filter((task) => task.status === 'ToDo');
  const inProgressTasks = boardTasks.filter((task) => task.status === 'InProgress');
  const doneTasks = boardTasks.filter((task) => task.status === 'Done');

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
              height: '100vh',
            }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', width: '100%', gap: 1 }}>
              <Typography
                  component="h1"
                  variant="h3"
                  sx={{ fontSize: 'clamp(1.25rem, 10vw, 1.5rem)', textAlign: 'left' }}
              >
                {board?.name}
              </Typography>
              <IconButton aria-label="Edit board" onClick={handleOpenUpdateBoard} size="small">
                <EditIcon />
              </IconButton>
            </Box>
            <Button type="button" variant="contained" onClick={handleOpenCreateTask}>+</Button>
          </Box>
          <Box sx={{ mt: 3, display: 'flex', gap: 2, height: 'calc(100vh - 93px)' }}>
            <Box sx={{ flex: 1, backgroundColor: 'rgb(31,31,33)', borderRadius: '10px', p: 1 }}>
              <Typography variant="body1" sx={{ padding: '10px 0', fontWeight: 'bold'}}>To-Do</Typography>
              <Divider />
              {toDoTasks.map((task) => (
                <Task
                  key={task.id}
                  id={task.id}
                  summary={task.summary}
                  dueDate={task.dueAt}
                  onClick={() => handleOpenUpdateTask(task)}
                />
              ))}
            </Box>
            <Box sx={{ flex: 1, backgroundColor: 'rgb(31,31,33)', borderRadius: '10px', p: 1 }}>
              <Typography variant="body1" sx={{ padding: '10px 0', fontWeight: 'bold' }}>In Progress</Typography>
              <Divider />
              {inProgressTasks.map((task) => (
                <Task
                  key={task.id}
                  id={task.id}
                  summary={task.summary}
                  dueDate={task.dueAt}
                  onClick={() => handleOpenUpdateTask(task)}
                />
              ))}
            </Box>
            <Box sx={{ flex: 1, backgroundColor: 'rgb(31,31,33)', borderRadius: '10px', p: 1 }}>
              <Typography variant="body1" sx={{ padding: '10px 0', fontWeight: 'bold' }}>Done</Typography>
              <Divider />
              {doneTasks.map((task) => (
                <Task
                  key={task.id}
                  id={task.id}
                  summary={task.summary}
                  dueDate={task.dueAt}
                  onClick={() => handleOpenUpdateTask(task)}
                />
              ))}
            </Box>
          </Box>
        </Box>
        <Dialog open={createTaskOpen} onClose={handleCloseCreateTask} fullWidth maxWidth="sm">
          <Box component="form" onSubmit={handleCreateTaskSubmit}>
            <DialogTitle>Create New Task</DialogTitle>
            <DialogContent>
              <TextField
                autoFocus
                fullWidth
                label="Summary"
                value={taskSummary}
                onChange={(event) => setTaskSummary(event.target.value)}
                margin="dense"
              />
              <TextField
                fullWidth
                label="Description"
                value={taskDescription}
                onChange={(event) => setTaskDescription(event.target.value)}
                margin="dense"
                multiline
                rows={3}
                sx={{
                  '& .MuiOutlinedInput-root': { height: 'auto' }
                }}
              />
              <LocalizationProvider dateAdapter={AdapterDayjs}>
                <DatePicker
                    value={taskDueDate}
                    onChange={(newTaskDueDate) => setTaskDueDate(newTaskDueDate)}
                    label="Due Date"
                    sx={{ backgroundColor: 'black', borderRadius: '10px', marginTop: '5px' }}
                />
              </LocalizationProvider>
              <TextField
                fullWidth
                select
                label="Status"
                value={taskStatus}
                onChange={(event) => setTaskStatus(event.target.value)}
                margin="dense"
              >
                <MenuItem value="ToDo">To Do</MenuItem>
                <MenuItem value="InProgress">In Progress</MenuItem>
                <MenuItem value="Done">Done</MenuItem>
              </TextField>
              {createTaskError && (
                <Typography variant="body2" color="error" sx={{ mt: 1 }}>
                  {createTaskError}
                </Typography>
              )}
            </DialogContent>
            <DialogActions sx={{ paddingRight: '20px' }}>
              <Button type="button" onClick={handleCloseCreateTask} disabled={isCreatingTask}>
                Cancel
              </Button>
              <Button type="submit" disabled={isCreatingTask || taskSummary.trim().length === 0}>
                Save
              </Button>
            </DialogActions>
          </Box>
        </Dialog>
        <Dialog open={updateTaskOpen} onClose={handleCloseUpdateTask} fullWidth maxWidth="sm">
          <Box component="form" onSubmit={handleUpdateTaskSubmit}>
            <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', pr: 2 }}>
              Update Task
              <Button
                type="button"
                variant="contained"
                color="error"
                onClick={handleOpenDeleteConfirm}
                disabled={isUpdatingTask || isDeletingTask}
              >
                Delete
              </Button>
            </DialogTitle>
            <DialogContent>
              <TextField
                autoFocus
                fullWidth
                label="Summary"
                value={updateTaskSummary}
                onChange={(event) => setUpdateTaskSummary(event.target.value)}
                margin="dense"
              />
              <TextField
                fullWidth
                label="Description"
                value={updateTaskDescription}
                onChange={(event) => setUpdateTaskDescription(event.target.value)}
                margin="dense"
                multiline
                rows={3}
                sx={{
                  '& .MuiOutlinedInput-root': { height: 'auto' }
                }}
              />
              <LocalizationProvider dateAdapter={AdapterDayjs}>
                <DatePicker
                  value={updateTaskDueDate}
                  onChange={(newTaskDueDate) => setUpdateTaskDueDate(newTaskDueDate)}
                  label="Due Date"
                  sx={{ backgroundColor: 'black', borderRadius: '10px', marginTop: '5px' }}
                />
              </LocalizationProvider>
              <TextField
                fullWidth
                select
                label="Status"
                value={updateTaskStatus}
                onChange={(event) => setUpdateTaskStatus(event.target.value)}
                margin="dense"
              >
                <MenuItem value="ToDo">To Do</MenuItem>
                <MenuItem value="InProgress">In Progress</MenuItem>
                <MenuItem value="Done">Done</MenuItem>
              </TextField>
              {updateTaskError && (
                <Typography variant="body2" color="error" sx={{ mt: 1 }}>
                  {updateTaskError}
                </Typography>
              )}
            </DialogContent>
            <DialogActions sx={{ paddingRight: '20px' }}>
              <Button type="button" onClick={handleCloseUpdateTask} disabled={isUpdatingTask || isDeletingTask}>
                Cancel
              </Button>
              <Button type="submit" disabled={isUpdatingTask || isDeletingTask || updateTaskSummary.trim().length === 0}>
                Save
              </Button>
            </DialogActions>
          </Box>
        </Dialog>
        <Dialog open={deleteConfirmOpen} onClose={handleCloseDeleteConfirm} fullWidth maxWidth="xs">
          <DialogTitle>Are you sure you want to delete this task?</DialogTitle>
          <DialogActions sx={{ paddingRight: '20px' }}>
            <Button type="button" onClick={handleCloseDeleteConfirm} disabled={isDeletingTask}>
              No
            </Button>
            <Button type="button" color="error" onClick={handleDeleteTaskConfirm} disabled={isDeletingTask}>
              Yes
            </Button>
          </DialogActions>
        </Dialog>
        <Dialog open={updateBoardOpen} onClose={handleCloseUpdateBoard} fullWidth maxWidth="sm">
          <Box component="form" onSubmit={handleUpdateBoardSubmit}>
            <DialogTitle>Update Board</DialogTitle>
            <DialogContent>
              <TextField
                autoFocus
                fullWidth
                label="Board Name"
                value={updateBoardName}
                onChange={(event) => setUpdateBoardName(event.target.value)}
                margin="dense"
              />
              {updateBoardError && (
                <Typography variant="body2" color="error" sx={{ mt: 1 }}>
                  {updateBoardError}
                </Typography>
              )}
            </DialogContent>
            <DialogActions sx={{ paddingRight: '20px' }}>
              <Button type="button" onClick={handleCloseUpdateBoard} disabled={isUpdatingBoard}>
                Cancel
              </Button>
              <Button type="submit" disabled={isUpdatingBoard || updateBoardName.trim().length === 0}>
                Save
              </Button>
            </DialogActions>
          </Box>
        </Dialog>
      </Box>
  );
}
