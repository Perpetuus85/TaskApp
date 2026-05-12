import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react';
import { axiosPrivate } from '../api/axios';

export type BoardTask = {
  id: string;
  summary: string;
  dueAt: string;
}

export type Board = {
  id: number;
  name: string;
  boardTasks?: BoardTask[];
};

export type CreateBoardPayload = {
  name: string;
};

export type UpdateBoardPayload = {
  id: string;
  name: string;
};

export type CreateBoardTaskPayload = {
  summary: string;
  description: string;
  dueDate?: string;
  status: string;
  boardId: string;
};

export type UpdateBoardTaskPayload = {
  id: string;
  summary: string;
  description: string;
  dueDate?: string;
  status: string;
};

export type DeleteBoardTaskPayload = {
  id: string;
};

type BoardContextType = {
  boards: Board[];
  setBoards: (value: Board[] | ((previous: Board[]) => Board[])) => void;
  GetAllBoards: () => Promise<Board[]>;
  CreateBoard: (payload: CreateBoardPayload) => Promise<void>;
  UpdateBoard: (payload: UpdateBoardPayload) => Promise<void>;
  CreateBoardTask: (payload: CreateBoardTaskPayload) => Promise<void>;
  UpdateBoardTask: (payload: UpdateBoardTaskPayload) => Promise<void>;
  DeleteBoardTask: (payload: DeleteBoardTaskPayload) => Promise<void>;
  GetBoardWithTasks: (id: string) => Promise<Board>;
};

export const BoardContext = createContext<BoardContextType | undefined>(undefined);

export const useBoard = () => {
  const context = useContext(BoardContext);
  if (!context) {
    throw new Error('useBoard must be used within a BoardProvider');
  }
  return context;
};

export default function BoardProvider({ children }: { children: ReactNode }) {
  const [boards, setBoards] = useState<Board[]>([]);

  const GetAllBoards = useCallback(async () => {
    const response = await axiosPrivate.get<Board[]>('/board/getall');
    const nextBoards = response.data ?? [];
    setBoards(nextBoards);
    return nextBoards;
  }, []);

  const CreateBoard = useCallback(async (payload: CreateBoardPayload) => {
    await axiosPrivate.post('/Board/Create', payload);
  }, []);

  const UpdateBoard = useCallback(async (payload: UpdateBoardPayload) => {
    await axiosPrivate.post('/board/update', payload);
  }, []);

  const GetBoardWithTasks = useCallback(async (id: string) => {
    const response = await axiosPrivate.get<Board>(`/Board/GetBoardWithTasksById/${id}`, {
      withCredentials: true,
    });
    return response.data;
  }, []);

  const CreateBoardTask = useCallback(async (payload: CreateBoardTaskPayload) => {
    await axiosPrivate.post('/BoardTask/Create', payload, {
      withCredentials: true,
    });
    //await GetBoardWithTasks(payload.boardId);
  }, []);

  const UpdateBoardTask = useCallback(async (payload: UpdateBoardTaskPayload) => {
    await axiosPrivate.post('/boardtask/update', payload, {
      withCredentials: true,
    });
  }, []);

  const DeleteBoardTask = useCallback(async (payload: DeleteBoardTaskPayload) => {
    await axiosPrivate.post('/boardtask/delete', payload, {
      withCredentials: true,
    });
  }, []);

  const contextValue = useMemo<BoardContextType>(
    () => ({
      boards,
      setBoards,
      GetAllBoards,
      CreateBoard,
      UpdateBoard,
      CreateBoardTask,
      UpdateBoardTask,
      DeleteBoardTask,
      GetBoardWithTasks,
    }),
    [boards, CreateBoard, UpdateBoard, CreateBoardTask, UpdateBoardTask, DeleteBoardTask, GetAllBoards, GetBoardWithTasks],
  );

  return <BoardContext.Provider value={contextValue}>{children}</BoardContext.Provider>;
}
