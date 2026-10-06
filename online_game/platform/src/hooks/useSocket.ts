"use client";

import { useEffect, useRef, useState } from "react";
import { io, Socket } from "socket.io-client";
import type { ChatMessage, OnlineUser } from "@/types";

interface UseSocketProps {
  user: {
    id: string;
    username: string;
    avatar_url: string | null;
  } | null;
}

export function useSocket({ user }: UseSocketProps) {
  const socketRef = useRef<Socket | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [onlineUsers, setOnlineUsers] = useState<OnlineUser[]>([]);
  const [messages, setMessages] = useState<ChatMessage[]>([]);

  useEffect(() => {
    if (!user) return;

    const socketUrl = process.env.NEXT_PUBLIC_SOCKET_URL || "http://localhost:3001";
    socketRef.current = io(socketUrl);

    const socket = socketRef.current;

    socket.on("connect", () => {
      setIsConnected(true);
      socket.emit("join_lobby", {
        id: user.id,
        username: user.username,
        avatar_url: user.avatar_url,
      });
    });

    socket.on("disconnect", () => {
      setIsConnected(false);
    });

    socket.on("online_users", (users: OnlineUser[]) => {
      setOnlineUsers(users);
    });

    socket.on("chat_history", (history: ChatMessage[]) => {
      setMessages(history);
    });

    socket.on("user_joined", (newUser: OnlineUser) => {
      setOnlineUsers((prev) => [...prev, newUser]);
    });

    socket.on("user_left", (userId: string) => {
      setOnlineUsers((prev) => prev.filter((u) => u.id !== userId));
    });

    socket.on("new_message", (message: ChatMessage) => {
      setMessages((prev) => [...prev, message]);
    });

    return () => {
      socket.disconnect();
    };
  }, [user]);

  const sendMessage = (content: string) => {
    if (socketRef.current && user && content.trim()) {
      socketRef.current.emit("send_message", {
        user_id: user.id,
        username: user.username,
        avatar_url: user.avatar_url,
        content: content.trim(),
      });
    }
  };

  return {
    isConnected,
    onlineUsers,
    messages,
    sendMessage,
  };
}
