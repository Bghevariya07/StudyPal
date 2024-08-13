<template>
    <div class="flex">
        <SideBar class="" />
        <div class="w-full align-middle p-10">
            <div class="flex justify-start gap-10 items-center mb-4">
                <div class="mt-2 p-4 rounded flex gap-2 w-56 align-center">
                    <label for="role-select" class="m-auto">Role:</label>
                    <select id="role-select" v-model="role" class="p-2 w-full hover:bg-gray-200 rounded-lg">
                        <option value="tutor" class="p-5 w-full hover:bg-gray-200 rounded">Tutor</option>
                        <option value="student" class="p-2 w-full hover:bg-gray-200 rounded">Student</option>
                    </select>
                </div>
                <div @click="fetchCourses" class="w-72">
                    <div class="relative inline-block mt-2 bg-white p-4 rounded w-full flex">
                        <input type="text" v-model="searchQuery" placeholder="Search Courses... "
                            class="px-3 my-2 border rounded-lg focus:outline-none focus:border-blue-500 w-full mr-2" />
                        <button @click="toggleSearchBar" class="p-1 h-7 m-auto bg-blue-500 text-white rounded-full">
                            <img :src="CloseIcon" class="w-5 h-5" alt="new-chat">
                        </button>
                    </div>


                    <ul class="absolute bg-white overflow-y-auto h-min max-h-52">
                        <li v-for="course in filtered" @click="selectCourse(course)"
                            class="p-2 cursor-pointer hover:bg-gray-200"> {{ course }} </li>
                    </ul>
                </div>
                <h4 v-if="courseId !== ''">{{ courseId }}</h4>
            </div>
            <div class="flex flex-row gap-5">
                <div class="parent flex-2">
                    <div v-for="slot in slots" class="p-2 text-center">
                        <h5>{{ slot[0] }}</h5>
                        <div v-for="{ start, data } in slot[1]" :key="start.getTime()"
                            style="display: flex; flex-direction: column;" class="p-2 cursor-pointer"
                            @click="() => handleClick(start, data)"
                            :class="[data.isSelected ? 'bg-primary' : 'time-slot']">
                            <h6>{{ start.toLocaleString(undefined, {
                                hour: '2-digit', minute: '2-digit', second: '2-digit'
                            }) }}</h6>
                        </div>
                    </div>
                </div>
                <div>
                    <div class="py-10 space-y-2" v-if="selectedSlot?.data.isSelected && role === 'tutor'">
                        <p class="text-3xl font-semibold pb-2 border-b">Study Session:</p>
                        <p class="text-xl pt-1">{{ }}</p>
                        <p class="text-xl">{{ formatDate(selectedSlot?.start) }}</p>
                        <p class="text-xl">{{ formatTime(selectedSlot?.start, new Date(selectedSlot?.start.getTime() +
                            60 * 60 * 1000)) }}</p>
                        <br>
                        <button @click="cancelSession"
                            class="mx-2 bg-red-500 text-white py-2 px-4 rounded-full p-2">Cancel this Session</button>
                    </div>
                    <div class="py-10 space-y-2" v-if="!selectedSlot?.data.isSelected && role === 'tutor'">
                        <p class="text-3xl font-semibold pb-2 border-b">Study Session:</p>
                        <p class="text-xl pt-1">{{ }}</p>
                        <p class="text-xl">{{ formatDate(selectedSlot?.start) }}</p>
                        <p class="text-xl">{{ formatTime(selectedSlot?.start, new Date(selectedSlot?.start.getTime() +
                            60 * 60 * 1000)) }}</p>
                        <br>
                        <button @click="createSession"
                            class="mx-2 bg-green-500 text-white py-2 px-4 rounded-full p-2">Create a Session</button>
                    </div>
                    <div class="py-10 space-y-2" v-if="selectedSlot?.data.isSelected && role === 'student'">
                        <p class="text-3xl font-semibold pb-2 border-b">Study Session:</p>
                        <p class="text-xl pt-1">Tutor: {{ getUserDetails(selectedSlot.data.eventId)?.firstName + " " + getUserDetails(selectedSlot.data.eventId)?.lastName }}</p>
                        <p class="text-xl pb-1">Registered Students: {{ selectedSlot.data.users.length }}</p>
                        <p class="text-xl pt-2 border-b">{{ formatDate(selectedSlot?.start) }}</p>
                        <p class="text-xl">{{ formatTime(selectedSlot?.start, new Date(selectedSlot?.start.getTime() +
                            60 * 60 * 1000)) }}</p>
                        <br>
                        <button @click="joinSession" v-if="!selectedSlot.data.users.includes(userName)" class="mx-2 bg-primary text-white py-2 px-4 rounded-full p-2">Join
                            this Session</button>
                        <button @click="leaveSession" v-else
                            class="mx-2 bg-red-500 text-white py-2 px-4 rounded-full p-2">Leave this Session</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { useUserStore } from "@/store/user";
import SideBar from "../SideBar.vue";
import { ref, watch, onMounted, computed } from "vue";
import { days, getWeekStart, hours } from '@/lib/utils';
import axios from "axios";
import { TutorSchedule } from "@/models/Schedule";
import CloseIcon from "../../assets/plus-icon-1.svg";
import { UserProfile } from "@/models/user";

interface SlotDataSelected {
    isSelected: true;
    id: string;
    users: string[];
    eventId: string;
}

interface SlotDataUnselected {
    isSelected: false;
    eventId: string;
}

interface Slot {
    start: Date;
    data: SlotData;
}

type SlotData = SlotDataSelected | SlotDataUnselected;

export default {
    components: {
        SideBar
    },
    setup() {
        const role = ref<'tutor' | 'student'>('student');
        const userStore = useUserStore();
        const tutorSchedules = ref<TutorSchedule[]>([]);
        const searchQuery = ref('');
        const slots = ref(new Map<string, Slot[]>());
        const week = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
        const userName = userStore.storedUser?.username || '';
        const selectedSlot = ref<Slot>({
            start: new Date(),
            data: { isSelected: false, eventId: "" }
        });
        const courses = ref<string[]>([]);
        const courseId = ref('');
        const isSearchBoxOpen = ref(false);
        const isSlotSelected = ref(false);
        const filtered = ref<string[]>([]);
        const userProfiles = ref<UserProfile[]>([]);

        const formatDate = (date: Date) => {
            const options: Intl.DateTimeFormatOptions = { month: 'long', day: 'numeric', year: 'numeric' };
            return date.toLocaleDateString('en-US', options);
        }

        const formatTime = (start: Date, end: Date) => {
            const options: Intl.DateTimeFormatOptions = { hour: 'numeric', minute: 'numeric' };
            const startTime = start.toLocaleTimeString('en-US', options);
            const endTime = end.toLocaleTimeString('en-US', options);
            return `${startTime} - ${endTime}`;
        }

        const onViewChanged = (newRole: 'tutor' | 'student') => {
            fetchTutorSchedules();

            if (userName && courseId) {

                if (newRole === "student") {
                    tutorSchedules.value = tutorSchedules.value.filter(i => i.username !== userName && i.courseId === courseId.value);
                } else if (newRole === "tutor") {
                    tutorSchedules.value = tutorSchedules.value.filter(i => i.username === userName && i.courseId === courseId.value);
                }
            }

            isSlotSelected.value = false;
            slots.value = getSlots();
        };

        const fetchTutorSchedules = async () => {
            try {
                const response = await axios.get(`/api/tutorschedule`);
                tutorSchedules.value = response.data.map((schedule: any) => ({
                    userId: schedule.username,
                    timeFrom: schedule.timeFrom,
                    timeTo: schedule.timeTo,
                    courseId: schedule.courseId,
                    eventId: schedule.eventId,
                    users: schedule.users
                }));

                tutorSchedules.value.filter((item: TutorSchedule) => { item.courseId === courseId.value })
            } catch (error) {
                console.error(error);
            }
        };

        const getSlots = (): Map<string, Slot[]> => {
            const start = new Date(getWeekStart().getTime() + 8 * hours);
            const slotsMap = new Map<string, Slot[]>();

            for (let i = 0; i < 7; i++) {
                const dayStart = start.getTime() + i * days;
                for (let j = dayStart; j < dayStart + 12 * hours; j += 1 * hours) {
                    const date = new Date(j);
                    const slotData = getIsSelected(date);
                    if (!slotsMap.has(week[i])) {
                        slotsMap.set(week[i], []);
                    }

                    const slot: Slot = {
                        start: date,
                        data: slotData,
                    };
                    slotsMap.get(week[i])?.push(slot);
                }
            }

            return slotsMap;
        };

        const fillSlots = () => {
            fetchTutorSchedules();

            if (userName && role.value === "student") {
                tutorSchedules.value = tutorSchedules.value.filter(i => i.username !== userName && i.courseId === courseId.value);
            } else {
                tutorSchedules.value = tutorSchedules.value.filter(i => i.username === userName && i.courseId === courseId.value);
            }

            slots.value = getSlots();
        }

        function createEventId(courseId: string, date: Date, username: string): string {
            const startTime = date.getTime();
            const endTime = startTime + (60 * 60 * 1000);
            return `${courseId}-${username}-${startTime}-${endTime}`;
        }

        const getIsSelected = (date: Date): SlotData => {
            const schedule = tutorSchedules.value.find((i: TutorSchedule) => i.timeFrom === date.getTime());
            if (schedule) {
                const users = tutorSchedules.value
                    ?.filter(i => i.timeFrom <= date.getTime() && i.timeTo > date.getTime())
                    .map(schedule => schedule.username);
                if (users) {
                    return { isSelected: true, id: schedule.username, users, eventId: schedule.eventId };
                }
            }

            if (schedule?.eventId) {
                return { isSelected: false, eventId: schedule?.eventId };
            } else {
                return { isSelected: false, eventId: "" };
            }
        }

        const handleClick = async (slot: Date, data: SlotData) => {
            isSlotSelected.value = true;
            console.log(data)
            selectedSlot.value = { start: slot, data: data };
            await fetchTutorSchedules();
        };

        const leaveSession = async () => {
            try {
                // const eventId = createEventId(courseId.value, selectedSlot.value.start, );
                // console.log(eventId);
                isSlotSelected.value = false;

                await axios.put(`api/tutorSchedule/removeuser/${selectedSlot.value.data.eventId}/${userName}`);
                await fillSlots();
            } catch (error) {
                console.log(error);
            }
        }

        const getUserDetails = (eventId: string) => {
            const parts = eventId.split('-');
            return userProfiles.value.find((i : UserProfile) => i.username == parts[1]);
        }

        const joinSession = async () => {
            try {
                // const eventId = createEventId(courseId.value, selectedSlot.value.start, user);
                // console.log(eventId);
                isSlotSelected.value = false;

                await axios.put(`api/tutorSchedule/adduser/${selectedSlot.value.data.eventId}/${userName}`);
                await fillSlots();
            } catch (error) {
                console.log(error);
            }
        }

        const createSession = async () => {
            try {
                let newSession: TutorSchedule = {
                    eventId: createEventId(courseId.value, selectedSlot.value.start, userName),
                    username: userName,
                    courseId: courseId.value,
                    timeFrom: selectedSlot.value.start.getTime(),
                    timeTo: selectedSlot.value.start.getTime() + (60 * 60 * 1000),
                    users: []
                }

                isSlotSelected.value = false;

                await axios.post(`api/tutorSchedule`, newSession);
                await fillSlots();
            } catch (error) {
                console.log(error);
            }
        }

        const fetchCourses = async () => {
            try {
                isSearchBoxOpen.value = true;

                const response = await axios.get(`/api/course`); // Adjust the API endpoint as needed
                courses.value = response.data.map((course: any) => `${course.courseCode} : ${course.courseName}`);
            } catch (error) {
                console.error("Error fetching courses:", error);
            }
        };

        const selectCourse = (course: string) => {
            isSearchBoxOpen.value = true;
            const name = course.split(" : ")[0];
            courseId.value = name;
            filtered.value = [];
            searchQuery.value = '';
            isSlotSelected.value = false;
            fillSlots();

            fetchTutorSchedules();
        };

        const filteredCourses = () => {
            if (!searchQuery.value) {
                return courses.value; // Return the entire list if no search query is entered
            }

            const query = searchQuery.value.toLowerCase();
            const valyes = courses.value.filter((course: string) => course.toLowerCase().includes(query));
            return valyes;
        };

        const cancelSession = async () => {
            try {
                const eventId = createEventId(courseId.value, selectedSlot.value.start, userName);
                isSlotSelected.value = false;
                await axios.delete(`api/tutorSchedule/delete/${eventId}`);
                await fillSlots();
            } catch (error) {
                console.log(error);
            }
        };

        const toggleSearchBar = () => {
            if (isSearchBoxOpen.value) {
                isSearchBoxOpen.value = false;
                filtered.value = [];
                searchQuery.value = '';
            } else {
                isSearchBoxOpen.value = true;
            }
        }

        const fetchAllUsers = async () => {
            try {
                const response = await axios.get('api/Userprofile/profiles/');
                userProfiles.value = response.data;
            } catch (error) {
                console.error('Error fetching profiles:', error);
            }
        }

        watch(role, async (newRole) => {
            await onViewChanged(newRole);
        });

        watch(searchQuery, () => {
            if (searchQuery.value) {
                filtered.value = filteredCourses();
            } else {
                filtered.value = [];
            }
        });

        onMounted(async () => {
            await onViewChanged(role.value);

            setInterval(async () => {
                await fillSlots();
            }, 3000);

            setInterval(async () => {
                await fetchAllUsers();
            }, 8000);
        });

        return {
            role,
            slots,
            selectedSlot,
            handleClick,
            onViewChanged,
            fetchTutorSchedules,
            fillSlots,
            formatDate,
            formatTime,
            leaveSession,
            joinSession,
            cancelSession,
            createSession,
            searchQuery,
            fetchCourses,
            filteredCourses,
            selectCourse,
            isSearchBoxOpen,
            toggleSearchBar,
            filtered,
            CloseIcon,
            courseId,
            getUserDetails,
            userName
        };
    },
    async mounted() {
        this.role = "student";
        this.fillSlots()
    }
};
</script>

<style scoped>
.parent {
    display: grid;
    grid-template-columns: repeat(7, 1fr);
    grid-column-gap: 0px;
    grid-row-gap: 0px;
}

.time-slot {
    background-color: whitesmoke;
}

.time-slot:hover {
    background-color: gray;
    cursor: pointer;
}
</style>
