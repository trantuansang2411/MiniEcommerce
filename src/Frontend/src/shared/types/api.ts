export type ApiResponse<T> = {
  statusCode: number;
  message: string;
  data: T;
};

export type ApiError = Error & {
  statusCode?: number;
  errors?: Record<string, string[]>;
};
