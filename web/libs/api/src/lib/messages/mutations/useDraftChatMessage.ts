import { messagesClient } from "../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import {
   SendGroupChatMessageModel,
   toFormData,
} from "./useSendGroupChatMessageMutation";

export interface DraftChatMessageModel extends SendGroupChatMessageModel {}

const draftChatMessage = async (model: DraftChatMessageModel) => {
   const { status, data } = await messagesClient.postForm(
      `drafts`,
      toFormData(model),
      {
         headers: {},
      }
   );

   if (
      status === HttpStatusCode.BadRequest ||
      status === HttpStatusCode.NotFound
   ) {
      throw new Error("error");
   }

   return data;
};

export const useDraftChatMessage = () => {
   const client = useQueryClient();
   return useMutation<any, Error, DraftChatMessageModel, any>(
      draftChatMessage,
      {
         onError: console.error,
         onSuccess: (data) =>
            console.log("Chat message drafted successfully: " + data),
         onSettled: (res) => console.log(res),
      }
   );
};
