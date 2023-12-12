"use client";
import React, { useMemo, useRef, useState } from "react";
import { ChatifyFile } from "@components/chat-group/messages/editor/MessageTextEditor";
import { v4 as uuidv4 } from "uuid";

export function useSingleFileUpload() {
   const fileInputRef = useRef<HTMLInputElement>();
   const [selectedFile, setSelectedFile] = useState<File | null>();
   const fileUrl = useMemo(() => {
      if (!selectedFile) return "";
      return URL.createObjectURL(selectedFile);
   }, [selectedFile]);

   const normalizedFileName = useMemo(() => {
      if (!selectedFile) return "";
      const parts = selectedFile?.name.split(".");
      return `${parts
         .slice(0, parts.length - 1)
         .join("")
         .substring(0, 20)}.${parts.at(-1)}`;
   }, [selectedFile]);

   return { fileInputRef, selectedFile, setSelectedFile, fileUrl, normalizedFileName }

}

export function useFileUpload(initialFiles?: ChatifyFile[]) {
   const [attachedFiles, setAttachedFiles] = useState<ChatifyFile[]>(() => initialFiles ?? []);

   // Mapping of File ID -> File Object URL
   const attachedFilesUrls = useMemo<Map<string, string>>(
      () =>
         new Map<string, string>(
            attachedFiles.map((file) => [
               file.id,
               URL.createObjectURL(file.file),
            ]),
         ),
      [attachedFiles],
   );
   const fileUploadRef = useRef<HTMLInputElement>(null!);

   const handleRemoveFile = (id: string) =>
      setAttachedFiles((files) => files.filter((_) => _.id !== id));

   const clearFiles = () => setAttachedFiles([]);

   const handleFileUpload: React.ChangeEventHandler<HTMLInputElement> = async ({
                                                                                  target: { files },
                                                                               }) => {
      const newFiles = Array.from({ length: files.length }).map((_, i) =>
         files.item(i),
      );
      setAttachedFiles((files) => [
         ...files,
         ...newFiles.map((_) => new ChatifyFile(_, uuidv4())),
      ]);
   };

   return {
      attachedFiles,
      attachedFilesUrls,
      handleFileUpload,
      fileUploadRef,
      clearFiles,
      handleRemoveFile,
   };
}
