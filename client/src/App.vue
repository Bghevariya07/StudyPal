<script setup lang="ts">
import NavBar from './components/NavBar.vue'
import DashboardBar from './views/DashboardBar.vue';
</script>

<template>
  <main class="flex">
    <DashboardBar class="w-2/12 fixed"/>
    <NavBar class="w-10/12 fixed"/>

    <div class="flex flex-row-reverse align-end w-full">
      <router-view class=" w-10/12"/>
    </div>
  </main>
</template>