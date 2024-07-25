<template>
  <div class="flex flex-col items-center h-screen pt-20 bg-white border-r border-gray-200 fixed">
    <div class="w-full">
      <div class="flex items-center p-4 px-8 cursor-pointer hover:bg-gray-100"
        :class="{ 'bg-blue-500 text-white': activeItem === 'Home', 'text-gray-800': activeItem !== 'Home' }"
        @click="selectMenuItem('Home')">
        <img :src="HomeIcon" alt="dashboard-icon" class="w-6 h-6 mr-3" />
        Dashboard
      </div>
      <div class="flex items-center p-4 px-8 cursor-pointer hover:bg-gray-100"
        :class="{ 'bg-blue-500 text-white': activeItem === 'StudyPal', 'text-gray-800': activeItem !== 'StudyPal' }"
        @click="selectMenuItem('StudyPal')">
        <img :src="StudyIcon" alt="dashboard-icon" class="w-6 h-6 mr-3" />
        StudyPal
      </div>
      <div class="flex items-center p-4 px-8 cursor-pointer hover:bg-gray-100"
        :class="{ 'bg-blue-500 text-white': activeItem === 'Schedule', 'text-gray-800': activeItem !== 'Schedule' }"
        @click="selectMenuItem('Schedule')">
        <img :src="ScheduleIcon" alt="dashboard-icon" class="w-6 h-6 mr-3" />
        Schedule
      </div>
      <div class="flex items-center p-4 px-8 cursor-pointer hover:bg-gray-100"
        :class="{ 'bg-blue-500 text-white': activeItem === 'Chats', 'text-gray-800': activeItem !== 'Chats' }"
        @click="selectMenuItem('Chats')">
        <img :src="ChatsIcon" alt="dashboard-icon" class="w-6 h-6 mr-3" />
        Chats
      </div>
      <div class="flex items-center p-4 px-8 cursor-pointer hover:bg-gray-100"
        :class="{ 'bg-blue-500 text-white': activeItem === 'Settings', 'text-gray-800': activeItem !== 'Settings' }"
        @click="selectMenuItem('Settings')">
        <img :src="SettingsIcon" alt="dashboard-icon" class="w-6 h-6 mr-3" />
        Settings
      </div>
    </div>
  </div>
</template>

<script>
import HomeIcon from '../assets/home-icon-1.svg';
import StudyIcon from '../assets/group-icon-2.png';
import ScheduleIcon from '../assets/schedule-icon-1.svg';
import ChatsIcon from '../assets/chats-icon-1.svg';
import SettingsIcon from '../assets/settings-icon-1.svg';

export default {
  data() {
    return {
      activeItem: 'Home',
      HomeIcon,
      StudyIcon,
      ScheduleIcon,
      ChatsIcon,
      SettingsIcon
    };
  },
  methods: {
    selectMenuItem(item) {
      this.activeItem = item;
    },
  },
};
</script>

<style scoped></style>
