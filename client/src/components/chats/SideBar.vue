<template>
    <div class="flex w-1/3 justify-around flex-col">
      <div class="p-4 font-bold border rounded-tl-2xl flex justify-between">
        <h6 class="">Conversations</h6>
        <NewChat @open-chat="openChatWindow"/>
      </div>
  
      <div class="border-l h-full border-gray-300 bg-zinc-100 overflow-y-auto rounded-bl-2xl ">
        <div v-for="conversation in conversations" :key="conversation.id" @click="selectConversation(conversation)"
          class="flex items-center p-3 border-b border-gray-300 cursor-pointer hover:bg-gray-100"
          :class="{ 'bg-blue-100': conversation.id === selectedConversation.id }">
          <div class="w-10 h-10 rounded-full bg-gray-400 text-white flex items-center justify-center mr-3">
            {{ conversation.id.toUpperCase()[0] }}
          </div>
          <div class="w-8/12	">
            <p :class="{ 'font-bold': conversation.id === selectedConversation.id }" class="text-gray-800">
              {{ conversation.name }}
            </p>
            <p :class="{ 'font-bold': conversation.id === selectedConversation.id }" class="text-gray-500 text-sm">
              {{ getLastMessage(conversation.id).message }}
            </p>
          </div>
          <div class="ml-auto w-20 text-right text-gray-400 text-xs">{{
            formatTime(getLastMessage(conversation.id).time) }}</div>
        </div>
      </div>
    </div>
  </template>
  
  <script lang="ts">
  import NewChat from '../chats/NewChat.vue';
  
  export default {
    props: {
      conversations: Array,
      selectedConversation: Object
    },
    components: {
      NewChat
    },
    methods: {
      selectConversation(conversation) {
        this.$emit('selectConversation', conversation);
      },
      getLastMessage(userId) {
        const filteredMessages = this.$parent.messages.filter(
          message =>
            (message.senderId === this.$parent.currentUserId && message.receiverId === userId) ||
            (message.receiverId === this.$parent.currentUserId && message.senderId === userId)
        );
        return filteredMessages.sort((a, b) => new Date(b.time) - new Date(a.time))[0] || {};
      },
      formatTime(time) {
        const date = new Date(time);
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      },
      openChatWindow(username) {
        this.$emit('open-chat', username);
      }
    }
  };
  </script>
  