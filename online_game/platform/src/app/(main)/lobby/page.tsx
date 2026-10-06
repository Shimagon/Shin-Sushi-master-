"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { createClient } from "@/lib/supabase/client";
import { useSocket } from "@/hooks/useSocket";
import { OnlineUsersList } from "@/components/lobby/OnlineUsersList";
import { ChatBox } from "@/components/lobby/ChatBox";
import { LobbyHeader } from "@/components/lobby/LobbyHeader";
import type { User } from "@/types";

export default function LobbyPage() {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const router = useRouter();
  const supabase = createClient();

  const { isConnected, onlineUsers, messages, sendMessage } = useSocket({
    user: user
      ? {
          id: user.id,
          username: user.username,
          avatar_url: user.avatar_url,
        }
      : null,
  });

  useEffect(() => {
    const getUser = async () => {
      const {
        data: { user: authUser },
      } = await supabase.auth.getUser();

      if (!authUser) {
        router.push("/login");
        return;
      }

      const { data: profile } = await supabase
        .from("users")
        .select("*")
        .eq("id", authUser.id)
        .single();

      if (profile) {
        setUser(profile);
      }
      setLoading(false);
    };

    getUser();
  }, [supabase, router]);

  const handleLogout = async () => {
    await supabase.auth.signOut();
    router.push("/login");
    router.refresh();
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-pulse text-primary text-xl">Loading...</div>
      </div>
    );
  }

  if (!user) return null;

  return (
    <div className="min-h-screen flex flex-col">
      <LobbyHeader
        user={user}
        isConnected={isConnected}
        onLogout={handleLogout}
      />
      <main className="flex-1 container mx-auto p-4 grid grid-cols-1 lg:grid-cols-4 gap-4">
        <div className="lg:col-span-1">
          <OnlineUsersList users={onlineUsers} currentUserId={user.id} />
        </div>
        <div className="lg:col-span-3">
          <ChatBox
            messages={messages}
            currentUserId={user.id}
            onSendMessage={sendMessage}
          />
        </div>
      </main>
    </div>
  );
}
