import {useAuth} from "../context/AuthProvider.tsx";
import {Navigate, Route, Routes} from "react-router";
import SignIn from "../components/sign-in/SignIn.tsx";
import SignUp from "../components/sign-up/SignUp.tsx";
import Home from "../components/home/Home.tsx";
import BoardList from "../components/boards/BoardList.tsx";
import Board from "../components/boards/Board.tsx";
import {Suspense} from "react";

const AppRoutes = () => {
    const { isAuthenticated } = useAuth();

    return (
        <Suspense fallback={null}>
            <Routes>
                <Route path="/" element={isAuthenticated ? (
                    <Home />
                ) : (
                    <Navigate to="/sign-in" replace/>
                )
                } >
                    <Route index element={<BoardList />} />
                    <Route path="boards" element={<BoardList />} />
                    <Route path="boards/:boardId" element={<Board />} />
                </Route>
                <Route path="sign-up" element={<SignUp />} />
                <Route path="sign-in" element={<SignIn />} />
                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </Suspense>
    );
}

export default AppRoutes;
