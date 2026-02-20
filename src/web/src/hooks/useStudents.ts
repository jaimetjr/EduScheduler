import { useQuery } from '@tanstack/react-query';
import api from '../api/client';
import type { PaginatedResponse, StudentDto } from '../types';

export function useStudents(search: string, page: number, pageSize: number = 20) {
  return useQuery({
    queryKey: ['students', search, page, pageSize],
    queryFn: async () => {
      const params = new URLSearchParams();
      if (search) params.append('search', search);
      params.append('page', page.toString());
      params.append('pageSize', pageSize.toString());

      const response = await api.get<PaginatedResponse<StudentDto>>(
        `/students?${params.toString()}`
      );
      return response.data;
    },
  });
}
