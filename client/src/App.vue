<script setup lang="ts">
import NavBar from './components/NavBar.vue'
</script>

<template>
  <main class="flex">
    <NavBar class="fixed"/>

    <div class="w-full">
      <router-view/>
    </div>
  </main>
</template>