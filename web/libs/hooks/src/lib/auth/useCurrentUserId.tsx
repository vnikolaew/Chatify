"use client";
import { useGetMyClaimsQuery } from "@web/api";

export function useCurrentUserId() {
   const { data: me } = useGetMyClaimsQuery();
   return me?.claims?.nameidentifier;
}

export function useCurrentUsername() {
   const { data: me } = useGetMyClaimsQuery();
   return me?.claims?.name;
}
