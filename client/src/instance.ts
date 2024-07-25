import axios from "axios";

export const server = axios.create({
    baseURL: "https://studypal-s60l.onrender.com/api/"
});
