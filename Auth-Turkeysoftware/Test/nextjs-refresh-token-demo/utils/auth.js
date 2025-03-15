import axios from "axios";

const API_URL = "https://localhost:7157/api/";

const api = axios.create({
  baseURL: API_URL,
  withCredentials: true, // Ensure cookies are sent with every request
});

// Request interceptor (no need to attach token manually when using HttpOnly cookies)
api.interceptors.request.use(
  (config) => {
    return config;
  },
  (error) => Promise.reject(error)
);

let isRefreshing = false;

api.interceptors.response.use(
  (response) => {
    isRefreshing = false;
    return response;
  },
  async (error) => {
    const originalRequest = error.config;

    if (error.status === 401) {
      try {
        if(isRefreshing) {
            isRefreshing = false;
            window.location.href = '/';
            return Promise.reject(error);
        }

        if(!isRefreshing) {
            isRefreshing = true;
            await refreshToken();
        }
        return await api(originalRequest);
      } catch (err) {
        isRefreshing = false;
        window.location.href = '/'; 
        return Promise.reject(err);
      }
    } 
    isRefreshing = false;
    return Promise.reject(error);
  }
);

// API functions
export const registerUser = async (email, username, password) => {
  try {
    const response = await api.post("auth/Register/register-user", { email, username, password });
    return response.data;
  } catch (e) {
    throw new Error("Registration failed!");
  }
};

export const loginUser = async (email, password) => {
  try {
    const response = await api.post("auth/Login/login/", { email, password });
    return response.data;
  } catch (e) {
    throw new Error("Login failed!");
  }
};

export const logoutUser = async () => {
  try {
    const response = await api.post("Users/logout/");
    return response.data;
  } catch (e) {
    throw new Error("Logout failed!");
  }
};

export const getUserInfo = async () => {
  try {
    const response = await api.get("Users/get-info/");
    return response.data;
  } catch (e) {
    throw new Error("Getting user info failed!");
  }
};

export const refreshToken = async () => {
  try {
    const response = await api.post("auth/Login/refresh-token/");
    return response.data;
  } catch (e) {
    throw new Error("Refreshing token failed!");
  }
};

