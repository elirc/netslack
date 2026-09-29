import { HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import { useEffect } from "react";
import { API_BASE_URL } from "../api/http";
import type { Message } from "../api/types";

export function useSignalRChannel(channelId: string | null, accessToken: string | null, onMessage: (message: Message) => void) {
  useEffect(() => {
    if (!channelId || !accessToken) {
      return;
    }

    let disposed = false;
    const connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/messages`, { accessTokenFactory: () => accessToken })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    connection.on("MessagePosted", (message: Message) => {
      onMessage(message);
    });

    async function connect() {
      await connection.start();
      if (!disposed && connection.state === HubConnectionState.Connected) {
        await connection.invoke("JoinChannel", channelId);
      }
    }

    connect().catch((error: unknown) => {
      console.error("SignalR connection failed", error);
    });

    return () => {
      disposed = true;
      if (connection.state === HubConnectionState.Connected) {
        void connection.invoke("LeaveChannel", channelId).finally(() => connection.stop());
      } else {
        void connection.stop();
      }
    };
  }, [accessToken, channelId, onMessage]);
}

