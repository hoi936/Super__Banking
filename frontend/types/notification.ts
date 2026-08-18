export interface NotificationItem {
    id: string;
    title: string;
    message: string;
    type: string;
    isRead: boolean;
    createdAtUtc: string;
    readAtUtc?: string;
}

export interface UnreadNotificationCount {
    count: number;
}
