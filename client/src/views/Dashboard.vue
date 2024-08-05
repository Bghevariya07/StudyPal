<!-- TESTING PURPOSE -->
<template>
  <div class="flex">
    <SideBar />
    <div class="p-6 gap-10 flex flex-col w-4/5">
      <div class="flex row">
        <h4>Welcome Back,</h4>
        <h4 class="text-blue pl-2"> {{ username }}!</h4>
      </div>

      <div class="flex p-3">
        <div class="bg-cyan-100 p-5 rounded-2xl list-none flex flex-col">
          <h6>{{ formatTime(new Date()) }}</h6>
          <div v-for="scheduleItem in filterScheduleByDate()" class="">
            <li class="pt-1">{{ formatScheduleItem(scheduleItem.timeFrom, scheduleItem.timeTo, scheduleItem.courseId) }}
            </li>
          </div>
        </div>
        <div class="bg-cyan-100">

        </div>
      </div>

      <div class="flex gap-5 bg-zinc-300 p-5 rounded-2xl overflow-x-auto">
        <div v-for="course in courseIds"
          class="bg-light-blue min-w-56 h-40 flex flex-col p-3 rounded-2xl hover:bg-gray-200">
          <h5 class="">{{ course + ":" }}</h5>
          <h6 class="text-lg">{{ courses.find(val => val.courseCode === course)?.courseName }}</h6>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts">
import { useUserStore } from "@/store/user";
import SideBar from "../components/SideBar.vue";
import { User } from "@/models/user";
import axios from "axios";
import { Course, ScheudleItem } from "@/models/Schedule";

export default {
  data() {
    return {
      user: null as User | null,
      username: "",
      userId: "",
      schedules: [] as ScheudleItem[],
      courseIds: [] as string[],
      courses: [] as Course[]
    }
  },
  components: {
    SideBar,
  },
  setup() {
    const userStore = useUserStore();
    return {
      userStore,
    };
  },
  methods: {
    async fetchCourseDetails() {
      try {
        const response = await axios.get("api/course");
        this.courses = response.data;
      } catch (error) {
        console.log(error);
      }
    },
    async fetchAllGroupSessions(id: string) {
      try {
        const response = await axios.get("api/schedule/" + id);
        this.schedules = response.data;
        this.courseIds = this.filterUniqueCourses();
        this.fetchCourseDetails();
      } catch (error) {
        console.log(error);
      }
    },
    filterUniqueCourses() {
      const courseIds = this.schedules.map((course: ScheudleItem) => course.courseId);
      return [...new Set(courseIds)];
    },
    filterScheduleByCourse(id: string) {
      return this.schedules.filter((schedule: ScheudleItem) => schedule.courseId === id);
    },
    filterScheduleByDate() {
      return this.schedules.filter((schedule: ScheudleItem) => new Date(schedule.timeFrom).getDate() === new Date().getDate()).sort((a, b) => a.timeFrom - b.timeFrom);
    },
    formatTime(time: Date) {
      const date = new Date(time);
      const options: Intl.DateTimeFormatOptions = {
        weekday: 'long',
        month: 'long',
        day: 'numeric'
      };
      const formattedDate = date.toLocaleDateString('en-US', options);
      return formattedDate;
    },
    formatScheduleItem(from: number, to: number, course: string) {
      const fromDate = new Date(from);
      const toDate = new Date(to);

      const val = fromDate.getHours() + ":00 - " + toDate.getHours() + ":00 " + course;
      return val;
    },
    async fetchUser() {
      try {
        const response = await axios.get("update/User/" + this.username);
        this.userId = response.data.id;
        this.fetchAllGroupSessions(this.userId);
      } catch (error) {
        console.log(error);
      }
    }
  },
  async mounted() {
    this.user = this.userStore.user;
    if (this.user?.username != null) {
      this.username = this.user.username;
      this.fetchUser();
    }
  }
};
</script>
