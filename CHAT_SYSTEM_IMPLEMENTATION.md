# Chat System Implementation Guide

## Overview

A real-time chat system with voice message support has been implemented between Customer mobile app and Admin users in the Web dashboard. The system uses SignalR for real-time messaging and integrates with the existing notification system.

## Architecture

### Backend Components

1. **Models**
   - `ChatConversation.cs` - Conversation between customer and admin
   - `ChatMessage.cs` - Individual chat messages (text and voice)

2. **Repositories**
   - `ChatConversationRepository.cs` - Data access for conversations
   - `ChatMessageRepository.cs` - Data access for messages

3. **Services**
   - `ChatService.cs` - Business logic for chat operations
   - `ChatCleanupService.cs` - Background service to delete messages older than 30 days

4. **Controllers**
   - `ChatController.cs` - REST API endpoints

5. **SignalR Hub**
   - `ChatHub.cs` - Real-time messaging hub

### Database Schema

#### ChatConversations Table
```sql
CREATE TABLE ChatConversations (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    CustomerId NVARCHAR(450) NOT NULL,
    AdminId NVARCHAR(450) NULL,
    LastMessageAt DATETIME2 NOT NULL,
    LastMessagePreview NVARCHAR(200) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (CustomerId) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (AdminId) REFERENCES AspNetUsers(Id)
);
```

#### ChatMessages Table
```sql
CREATE TABLE ChatMessages (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    ConversationId BIGINT NOT NULL,
    SenderId NVARCHAR(450) NOT NULL,
    MessageType NVARCHAR(20) NOT NULL, -- "Text" or "Voice"
    Content NVARCHAR(2000) NULL,
    VoiceFilePath NVARCHAR(500) NULL,
    VoiceDuration INT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    ReadAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (ConversationId) REFERENCES ChatConversations(Id) ON DELETE CASCADE,
    FOREIGN KEY (SenderId) REFERENCES AspNetUsers(Id)
);
```

## Setup Instructions

### 1. Database Migration

Run the following command to create the migration:

```bash
cd Backend/SoitMed
dotnet ef migrations add AddChatSystem
dotnet ef database update
```

### 2. Create Voice Storage Directory

Ensure the voice storage directory exists:

```bash
mkdir -p wwwroot/chat/voice
```

The directory structure will be automatically created per conversation: `wwwroot/chat/voice/{conversationId}/`

### 3. Install Dependencies

#### Customer Mobile App
```bash
cd Customer
npm install expo-av
```

### 4. Configuration

The chat system is already configured in `Program.cs`:
- ChatService is registered
- ChatHub is mapped to `/chatHub`
- ChatCleanupService runs daily at 2 AM

## API Endpoints

### REST Endpoints

- `GET /api/Chat/conversations` - Get all conversations (admin) or user's conversation (customer)
- `POST /api/Chat/conversations` - Create new conversation
- `GET /api/Chat/conversations/{id}` - Get conversation by ID
- `PUT /api/Chat/conversations/{id}/assign` - Assign admin to conversation (admin only)
- `GET /api/Chat/conversations/{id}/messages` - Get messages for conversation
- `POST /api/Chat/messages` - Send text message
- `POST /api/Chat/messages/voice` - Upload and send voice message (multipart/form-data)
- `PUT /api/Chat/conversations/{id}/read` - Mark messages as read
- `GET /api/Chat/conversations/{id}/unread-count` - Get unread message count

### SignalR Hub

**Hub URL**: `/chatHub`

**Client Methods**:
- `JoinConversation(conversationId)` - Join conversation room
- `LeaveConversation(conversationId)` - Leave conversation room
- `Typing(conversationId, isTyping)` - Send typing indicator

**Server Events**:
- `ReceiveMessage` - New message received
- `UserTyping` - Typing indicator received
- `MessagesRead` - Messages read receipt
- `ConversationUpdated` - Conversation metadata updated
- `ConversationAssigned` - Conversation assigned to admin

## Features

### Real-time Messaging
- Text messages sent instantly via SignalR
- Voice messages recorded, uploaded, and delivered in real-time
- Typing indicators
- Read receipts
- Message delivery status

### Voice Messages
- Record voice up to 5 minutes
- Supported formats: MP3, WAV, M4A, AAC, OGG
- Max file size: 10MB
- Automatic playback controls
- Duration display

### Notifications Integration
- New message notifications via existing NotificationService
- Push notifications for mobile when app is closed
- In-app notifications when app is open

### Auto Cleanup
- Messages older than 30 days are automatically deleted
- Runs daily at 2 AM via ChatCleanupService
- Voice files are also deleted

## Security

- JWT authentication required for all endpoints
- Role-based access: Customers can only see their conversations, Admins can see all
- Voice file size limits (max 10MB per file)
- File type validation (audio files only)
- XSS protection for text messages

## Usage

### Web Dashboard (Admin)

1. Navigate to `/chat` route
2. View list of all conversations
3. Select a conversation to view messages
4. Type and send text messages
5. Play voice messages received from customers

### Customer Mobile App

1. Navigate to chat screen (accessible from navigation)
2. Conversation is auto-created on first message
3. Type and send text messages
4. Hold microphone button to record voice message
5. Release to send voice message
6. Play received voice messages

## File Structure

### Backend
```
Backend/SoitMed/
├── Models/
│   ├── ChatConversation.cs
│   └── ChatMessage.cs
├── DTO/
│   └── ChatDTOs.cs
├── Services/
│   ├── ChatService.cs
│   ├── IChatService.cs
│   └── ChatCleanupService.cs
├── Repositories/
│   ├── ChatConversationRepository.cs
│   ├── IChatConversationRepository.cs
│   ├── ChatMessageRepository.cs
│   └── IChatMessageRepository.cs
├── Controllers/
│   └── ChatController.cs
└── Hubs/
    └── ChatHub.cs
```

### Web Dashboard
```
Web/src/
├── pages/
│   └── ChatPage.tsx
├── components/
│   └── chat/
│       ├── ChatList.tsx
│       ├── ChatWindow.tsx
│       ├── MessageBubble.tsx
│       └── VoiceMessagePlayer.tsx
├── services/
│   └── chat/
│       ├── chatApi.ts
│       └── chatSignalRService.ts
├── stores/
│   └── chatStore.ts
└── types/
    └── chat.types.ts
```

### Customer Mobile
```
Customer/
├── screens/
│   └── common/
│       └── ChatScreen.tsx
├── components/
│   └── chat/
│       ├── VoiceRecorder.tsx
│       ├── VoicePlayer.tsx
│       └── MessageBubble.tsx
├── services/
│   ├── chatService.ts
│   └── chatSignalRService.ts
└── stores/
    └── chatStore.ts
```

## Testing

### Test Scenarios

1. **Create Conversation**
   - Customer sends first message → Conversation auto-created
   - Admin can view conversation in list

2. **Send Text Message**
   - Customer sends text → Admin receives via SignalR
   - Admin sends text → Customer receives via SignalR

3. **Send Voice Message**
   - Customer records voice → Uploads → Admin receives
   - Admin can play voice message

4. **Read Receipts**
   - Mark messages as read → Other party sees read status

5. **Typing Indicators**
   - User types → Other party sees "Typing..."

6. **Notifications**
   - Offline user receives push notification
   - Online user receives in-app notification

7. **Auto Cleanup**
   - Messages older than 30 days are deleted automatically

## Troubleshooting

### SignalR Connection Issues
- Check authentication token is valid
- Verify hub URL is correct: `/chatHub`
- Check CORS settings in `Program.cs`

### Voice Message Issues
- Ensure `expo-av` is installed: `npm install expo-av`
- Check microphone permissions
- Verify file size is under 10MB

### Database Issues
- Run migrations: `dotnet ef database update`
- Check connection string in `appsettings.json`

## Notes

- Chat history is automatically deleted after 30 days
- Voice files are stored in `wwwroot/chat/voice/{conversationId}/`
- Conversations are auto-created when customer sends first message
- Admin assignment can be manual or automatic (future enhancement)

