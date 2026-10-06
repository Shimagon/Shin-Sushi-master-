"use client";

import { motion } from "framer-motion";
import { LogOut, Wifi, WifiOff, Coins } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import type { User } from "@/types";

interface LobbyHeaderProps {
  user: User;
  isConnected: boolean;
  onLogout: () => void;
}

export function LobbyHeader({ user, isConnected, onLogout }: LobbyHeaderProps) {
  return (
    <header className="border-b border-border/50 bg-card/30 backdrop-blur-xl sticky top-0 z-50">
      <div className="container mx-auto px-4 py-3 flex items-center justify-between">
        <motion.div
          initial={{ x: -20, opacity: 0 }}
          animate={{ x: 0, opacity: 1 }}
          className="flex items-center gap-4"
        >
          <h1 className="text-2xl font-bold bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent">
            GAME LOBBY
          </h1>
          <div className="flex items-center gap-2">
            {isConnected ? (
              <div className="flex items-center gap-1 text-accent text-sm">
                <Wifi className="h-4 w-4" />
                <span>Connected</span>
              </div>
            ) : (
              <div className="flex items-center gap-1 text-destructive text-sm">
                <WifiOff className="h-4 w-4" />
                <span>Disconnected</span>
              </div>
            )}
          </div>
        </motion.div>

        <motion.div
          initial={{ x: 20, opacity: 0 }}
          animate={{ x: 0, opacity: 1 }}
          className="flex items-center gap-4"
        >
          <div className="flex items-center gap-2 px-3 py-1.5 rounded-full bg-secondary/50 border border-border/50">
            <Coins className="h-4 w-4 text-yellow-500" />
            <span className="text-sm font-medium">{user.points.toLocaleString()}</span>
          </div>

          <div className="flex items-center gap-3">
            <Avatar className="h-8 w-8 border-2 border-primary/50">
              <AvatarImage src={user.avatar_url || undefined} />
              <AvatarFallback className="bg-primary/20 text-primary">
                {user.username.slice(0, 2).toUpperCase()}
              </AvatarFallback>
            </Avatar>
            <span className="text-sm font-medium hidden sm:inline">{user.username}</span>
          </div>

          <Button
            variant="ghost"
            size="icon"
            onClick={onLogout}
            className="text-muted-foreground hover:text-destructive"
          >
            <LogOut className="h-5 w-5" />
          </Button>
        </motion.div>
      </div>
    </header>
  );
}
