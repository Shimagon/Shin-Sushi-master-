import { createServer } from "http";
import { Server } from "socket.io";

const httpServer = createServer();
const io = new Server(httpServer, {
  cors: {
    origin: process.env.FRONTEND_URL || "http://localhost:3000",
    methods: ["GET", "POST"],
  },
});

interface OnlineUser {
  id: string;
  username: string;
  avatar_url: string | null;
  socketId: string;
}

interface ChatMessage {
  id: string;
  user_id: string;
  username: string;
  avatar_url: string | null;
  content: string;
  timestamp: Date;
}

const onlineUsers: Map<string, OnlineUser> = new Map();
const chatHistory: ChatMessage[] = [];
const MAX_CHAT_HISTORY = 100;

io.on("connection", (socket) => {
  console.log("User connected:", socket.id);

  // Handle user joining the lobby
  socket.on("join_lobby", (user: { id: string; username: string; avatar_url: string | null }) => {
    const onlineUser: OnlineUser = {
      ...user,
      socketId: socket.id,
    };

    onlineUsers.set(user.id, onlineUser);

    // Send current online users to the new user
    socket.emit("online_users", Array.from(onlineUsers.values()));

    // Send chat history to the new user
    socket.emit("chat_history", chatHistory);

    // Notify all users about the new user
    socket.broadcast.emit("user_joined", onlineUser);

    console.log(`${user.username} joined the lobby`);
  });

  // Handle chat messages
  socket.on("send_message", (data: { user_id: string; username: string; avatar_url: string | null; content: string }) => {
    const message: ChatMessage = {
      id: `${Date.now()}-${data.user_id}`,
      user_id: data.user_id,
      username: data.username,
      avatar_url: data.avatar_url,
      content: data.content,
      timestamp: new Date(),
    };

    // Add to chat history
    chatHistory.push(message);
    if (chatHistory.length > MAX_CHAT_HISTORY) {
      chatHistory.shift();
    }

    // Broadcast message to all users
    io.emit("new_message", message);
  });

  // Handle disconnection
  socket.on("disconnect", () => {
    let disconnectedUser: OnlineUser | null = null;

    for (const [userId, user] of onlineUsers.entries()) {
      if (user.socketId === socket.id) {
        disconnectedUser = user;
        onlineUsers.delete(userId);
        break;
      }
    }

    if (disconnectedUser) {
      io.emit("user_left", disconnectedUser.id);
      console.log(`${disconnectedUser.username} left the lobby`);
    }
  });
});

const PORT = process.env.PORT || 3001;

httpServer.listen(PORT, () => {
  console.log(`Socket.io server running on port ${PORT}`);
});
