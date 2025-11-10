import { useQuery } from '@tanstack/react-query';
import { authApi } from '../api/authApi';

export function usePasswordRequirementsQuery() {
  return useQuery({
    queryKey: ['password-requirements'],
    queryFn: () => authApi.getPasswordRequirements(),
    staleTime: Infinity,
  });
}
