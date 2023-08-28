import { chatGroupsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { DEFAULT_CACHE_TIME } from "../../../constants";

export interface CreateChatGroupModel {
   about?: string;
   name: string;
   file: Blob;
}

const createChatGroup = async (model: CreateChatGroupModel) => {
   const { status, data } = await chatGroupsClient.postForm(`/`, model, {
      headers: {
         "Content-Type": "multipart/form-data",
      },
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useCreateChatGroupMutation = () => {
   const client = useQueryClient();

   return useMutation(createChatGroup, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("Chat group created successfully: " + data),
      onSettled: (res) => console.log(res),
      cacheTime: DEFAULT_CACHE_TIME,
   });
};
