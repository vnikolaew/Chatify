import React, { Fragment } from "react";

export interface PageProps {}

const Page = async ({}: PageProps) => {
   return (
      <section
         className={`w-2/3 mx-auto font-light mt-8 flex flex-col items-start gap-2`}
      >
         <h1 className={`text-xl font-semibold self-center`}>
            Terms of Service for Chatify
         </h1>
         <p>
            Please read these Terms of Service ("Terms") carefully before using
            our web-based chat application (the "Service") operated by Chatify
            2023 . Your access to and use of the Service is conditioned upon
            your acceptance of and compliance with these Terms. By accessing or
            using the Service, you agree to be bound by these Terms. If you do
            not agree to abide by these Terms, please refrain from using the
            Service.{" "}
         </p>
         <TermsOfServiceSection
            heading={"1. Acceptance of Terms"}
            content={
               "By using the Service, you agree to comply with and be bound by these\n            Terms, as well as any additional terms and conditions and policies\n            referenced herein or available through hyperlinks. These Terms apply\n            to all users of the Service, including, but not limited to, users\n            who are browsers, customers, or contributors of content."
            }
         />
         <TermsOfServiceSection
            heading={"2. Use of the Service"}
            content={
               <ol type={"a"}>
                  <li>
                     Eligibility: You must be at least 13 years old to use the
                     Service. If you are under the age of 13, please do not use
                     the Service.
                  </li>
                  <li>
                     User Accounts: You may be required to create an account to
                     use certain features of the Service. You are responsible
                     for maintaining the confidentiality of your account
                     information and password. You agree to accept
                     responsibility for all activities that occur under your
                     account.
                  </li>
                  <li>
                     Prohibited Activities: You agree not to use the Service for
                     any illegal or unauthorized purpose, nor to violate any
                     laws in your jurisdiction (including but not limited to
                     copyright laws). You may not use the Service to transmit
                     harmful code or interfere with the integrity or performance
                     of the Service. You may not engage in any form of
                     harassment, spamming, or abusive behavior while using the
                     Service.
                  </li>
               </ol>
            }
         />
         <TermsOfServiceSection
            heading={"3. Content"}
            content={
               <ol type={"a"}>
                  <li>
                     **User Content:** Users may post, upload, or transmit
                     content through the Service. You retain ownership of your
                     content, but by posting, uploading, or transmitting it, you
                     grant us a worldwide, non-exclusive, royalty-free license
                     to use, reproduce, adapt, publish, translate, and
                     distribute your content.
                  </li>
                  <li>
                     **Content Guidelines:** You agree not to post content that
                     is defamatory, obscene, illegal, or violates the rights of
                     others, including privacy and intellectual property rights.
                  </li>
               </ol>
            }
         />
         <TermsOfServiceSection
            heading={"4. Privacy"}
            content={
               <ol>
                  <li>
                     a. **Privacy Policy:** Your use of the Service is also
                     governed by our Privacy Policy, which can be found [link to
                     Privacy Policy]. By using the Service, you consent to our
                     collection, use, and disclosure of your information as
                     described in the Privacy Policy.
                  </li>
               </ol>
            }
         />
         <TermsOfServiceSection
            heading={"5. Termination"}
            content={
               "We reserve the right to terminate or suspend your access to the Service without prior notice for any reason, including, but not limited to, breach of these Terms."
            }
         />
         <TermsOfServiceSection
            heading={"6. Disclaimer of Warranties"}
            content={
               'The Service is provided "as is" and "as available" without\n            warranties of any kind, whether express or implied. We do not\n            warrant that the Service will be error-free or uninterrupted.'
            }
         />
         <TermsOfServiceSection
            heading={"7. Limitation of Liability"}
            content={
               "We shall not be liable for any direct, indirect, incidental,\n            special, consequential, or punitive damages arising out of or in\n            connection with your use of the Service."
            }
         />
         <TermsOfServiceSection
            heading={"8. Changes to Terms"}
            content={
               "We reserve the right to update or modify these Terms at any time.\n            You are responsible for reviewing these Terms periodically to ensure\n            your compliance."
            }
         />
         <TermsOfServiceSection
            heading={"9. Governing Law"}
            content={
               "These Terms shall be governed by and construed in accordance with the laws of [Your Jurisdiction], without regard to its conflict of law principles."
            }
         />
         <TermsOfServiceSection
            heading={"10. Contact Information"}
            content={
               "If you have any questions about these Terms, please contact us at\n            [Your Contact Information]. By using the Service, you acknowledge\n            that you have read, understood, and agree to be bound by these Terms\n            of Service."
            }
         />
      </section>
   );
};

interface TermsOfServiceSectionProps {
   heading: string;
   content: React.ReactNode;
}

const TermsOfServiceSection = ({
   content,
   heading,
}: TermsOfServiceSectionProps) => {
   return (
      <Fragment>
         <h1 className={`mt-4 font-normal text-large `}>{heading}</h1>
         {typeof content === "string" ? (
            <p className={`text-small indent-4 leading-5`}>{content}</p>
         ) : (
            content
         )}
      </Fragment>
   );
};

export default Page;
