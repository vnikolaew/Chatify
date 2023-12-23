import { useSearchParams } from "next/navigation";

export function useSearchParam(paramName: string): string | null {
    const params = useSearchParams();
    return params.get(paramName);
}
