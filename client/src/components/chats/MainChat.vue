<template>
  <div class="flex justify-around main-window pt-20 p-10">
    <div class="flex w-screen">
      <SideBar
        :conversations="conversations"
        :selectedConversation="selectedConversation"
        @selectConversation="selectConversation"
        @open-chat="openChatWindow"
      />
      <ChatWindow
        :selectedConversation="selectedConversation"
        :currentUserId="currentUserId"
        :newMessage="newMessage"
        @sendMessage="sendMessage"
        @updateNewMessage="updateNewMessage"
      />
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import axios from 'axios';
import SideBar from './SideBar.vue';
import ChatWindow from './ChatWindow.vue';

axios.defaults.baseURL = 'http://localhost:5267';

interface Message {
  senderId: string;
  receiverId: string;
  message: string;
  time: string;
}

interface Conversation {
  id: string;
  name?: string;
  messages: Message[];
}

export default defineComponent({
  components: {
    SideBar,
    ChatWindow
  },
  data() {
    return {
      currentUserId: 'ghevariya',
      newMessage: '',
      messages: [] as Message[],
      conversations: [] as Conversation[],
      selectedConversation: null as Conversation | null,
      refreshInterval: null as ReturnType<typeof setInterval> | null
    };
  },
  methods: {
    async fetchAllMessages() {
      try {
        const response = await axios.get('/api/chat/messages');
        this.messages = response.data;
        this.conversations = this.getUniqueUserIds();
        if (this.selectedConversation?.id) {
          this.selectConversation(this.selectedConversation);
        }
      } catch (error) {
        console.error('Error fetching messages:', error);
      }
    },
    getUniqueUserIds(): Conversation[] {
      const userMap = new Map<string, string>();

      this.messages.forEach(message => {
        if (message.senderId !== this.currentUserId && this.filterMessagesBySelectedUser(message.senderId).length !== 0) {
          if (!userMap.has(message.senderId) || new Date(userMap.get(message.senderId)!) < new Date(message.time)) {
            userMap.set(message.senderId, message.time);
          }
        }
        if (message.receiverId !== this.currentUserId && this.filterMessagesBySelectedUser(message.receiverId).length !== 0) {
          if (!userMap.has(message.receiverId) || new Date(userMap.get(message.receiverId)!) < new Date(message.time)) {
            userMap.set(message.receiverId, message.time);
          }
        }
      });

      const userArray = Array.from(userMap, ([id, time]) => ({ id, time }));

      userArray.sort((a, b) => new Date(b.time).getTime() - new Date(a.time).getTime());

      return userArray.map(user => ({
        id: user.id,
        name: this.getConversationName(user.id),
        messages: this.filterMessagesBySelectedUser(user.id)
      }));
    },
    getConversationName(userId: string): string {
      const user = this.conversations.find(conversation => conversation.id === userId);
      return user ? user.name || userId : userId;
    },
    getLastMessage(userId: string): Message | {} {
      const filteredMessages = this.messages.filter(
        message =>
          (message.senderId === this.currentUserId && message.receiverId === userId) ||
          (message.receiverId === this.currentUserId && message.senderId === userId)
      );
      return filteredMessages.sort((a, b) => new Date(b.time).getTime() - new Date(a.time).getTime())[0] || {};
    },
    selectConversation(conversation: Conversation) {
      this.selectedConversation = {
        id: conversation.id,
        name: conversation.name,
        messages: this.filterMessagesBySelectedUser(conversation.id)
      };
    },
    filterMessagesBySelectedUser(selectedUserId: string): Message[] {
      return this.messages.filter(
        message =>
          (message.senderId === this.currentUserId && message.receiverId === selectedUserId) ||
          (message.receiverId === this.currentUserId && message.senderId === selectedUserId)
      );
    },
    async sendMessage() {
      if (this.newMessage.trim() === '') return;
      const message: Message = {
        senderId: this.currentUserId,
        receiverId: this.selectedConversation!.id,
        message: this.newMessage,
        time: new Date().toISOString()
      };

      try {
        await axios.post('/api/chat/send', message);
        this.selectedConversation!.messages.push({
          ...message,
          senderId: this.currentUserId
        });
        this.newMessage = '';
        this.scrollToEnd();
      } catch (error) {
        console.error('Error sending message:', error);
      }
    },
    updateNewMessage(newMessage: string) {
      this.newMessage = newMessage;
    },
    scrollToEnd() {
      this.$nextTick(() => {
        const container = this.$refs.messagesContainer as HTMLElement | null;
        if (container) {
          container.scrollTop = container.scrollHeight;
        }
      });
    },
    openChatWindow(username: string) {
      const conversation: Conversation = { id: username, name: username, messages: [] };
      this.scrollToEnd();
      this.selectConversation(conversation);
    },
    startMessageRefresh() {
      this.refreshInterval = setInterval(this.fetchAllMessages, 5000); // Refresh every 5 seconds
    },
    stopMessageRefresh() {
      if (this.refreshInterval) {
        clearInterval(this.refreshInterval);
      }
    }
  },
  async mounted() {
    await this.fetchAllMessages();
    this.scrollToEnd();
    this.startMessageRefresh();
  },
  beforeUnmount() {
    this.stopMessageRefresh();
  }
});
</script>

<style>
.main-window {
  height: 100vh;
}
</style>
