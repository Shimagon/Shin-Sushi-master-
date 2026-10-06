"use client";

import { motion, AnimatePresence } from "framer-motion";
import { Users } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { ScrollArea } from "@/components/ui/scroll-area";
import type { OnlineUser } from "@/types";

interface OnlineUsersListProps {
  users: OnlineUser[];
  currentUserId: string;
}

export function OnlineUsersList({ users, currentUserId }: OnlineUsersListProps) {
  return (
    <Card className="border-border/50 bg-card/50 backdrop-blur-xl h-[calc(100vh-8rem)]">
      <CardHeader className="pb-3">
        <CardTitle className="text-lg flex items-center gap-2">
          <Users className="h-5 w-5 text-primary" />
          <span>Online</span>
          <span className="ml-auto text-sm font-normal text-muted-foreground">
            {users.length}
          </span>
        </CardTitle>
      </CardHeader>
      <CardContent className="p-0">
        <ScrollArea className="h-[calc(100vh-12rem)] px-4">
          <AnimatePresence mode="popLayout">
            {users.map((user) => (
              <motion.div
                key={user.id}
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: -20 }}
                className={`flex items-center gap-3 p-2 rounded-lg mb-2 transition-colors ${
                  user.id === currentUserId
                    ? "bg-primary/10 border border-primary/20"
                    : "hover:bg-secondary/50"
                }`}
              >
                <div className="relative">
                  <Avatar className="h-10 w-10 border-2 border-transparent">
                    <AvatarImage src={user.avatar_url || undefined} />
                    <AvatarFallback className="bg-secondary text-secondary-foreground">
                      {user.username.slice(0, 2).toUpperCase()}
                    </AvatarFallback>
                  </Avatar>
                  <div className="absolute -bottom-0.5 -right-0.5 h-3 w-3 bg-accent rounded-full border-2 border-card" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium truncate">
                    {user.username}
                    {user.id === currentUserId && (
                      <span className="text-xs text-muted-foreground ml-1">(You)</span>
                    )}
                  </p>
                </div>
              </motion.div>
            ))}
          </AnimatePresence>
        </ScrollArea>
      </CardContent>
    </Card>
  );
}
