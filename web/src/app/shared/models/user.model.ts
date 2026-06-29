export interface AuthResponse {
  token: string;
  user: UserDto;
}

export interface UserDto {
  id: string;
  email: string;
  createdAt: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}
