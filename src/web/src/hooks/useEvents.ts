import { useQuery } from '@tanstack/react-query';
import api from '../api/client';
import type { PaginatedResponse, StudentEventDto } from '../types';

export function useEvents(studentId: number | null, page: number, pageSize: number = 20) {
  return useQuery({
    queryKey: ['events', studentId, page, pageSize],
    queryFn: async () => {
      const params = new URLSearchParams();
      params.append('page', page.toString());
      params.append('pageSize', pageSize.toString());

      const response = await api.get<PaginatedResponse<StudentEventDto>>(
        `/students/${studentId}/events?${params.toString()}`
      );
      return response.data;
    },
    enabled: !!studentId,
  });
}
