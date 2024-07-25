import { createRouter, createWebHistory } from 'vue-router'
import Contact from './components/contact/Contact.vue'
import Homepage from './components/Homepage.vue'
import FAQ from "./views/FAQ.vue"
import Login from "./views/Login.vue"
import Register from "./views/Register.vue"
import Dashboard from './views/Dashboard.vue'
import { useUserStore } from './store/user'
import MainChat from './components/chats/MainChat.vue'


const routes = [
    { 
      path: '/', component: Homepage
    },
    { 
      path: '/faq', component: FAQ
    },
    { 
      path: '/login', component: Login
    },
    { 
      path: '/register', component: Register
    },
    { 
      path: '/contact', component: Contact
    },
    { 
      path: '/dashboard', component: Dashboard
    },
    { 
      path: '/chats', component: MainChat
    }
]

const router = createRouter({
    history: createWebHistory(),
    routes,
  });

  router.beforeEach(async (to) => {
    // redirect to login page if not logged in and trying to access a restricted page
    const publicPages = ['/login', '/faq', '/register', '/contact' ,'/', '/chats'];
    const authRequired = !publicPages.includes(to.path);
    const auth = useUserStore();

    if (authRequired && !auth.user) {
      return '/login';
    }
});

export default router;
