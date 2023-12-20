import React from "react";

export interface PageProps {

}

const Page = ({}: PageProps) => {
   return (
      <section className={`min-h-[70vh] mx-auto w-1/2 flex items-center flex-col m-8`}>
         <h2 className={`text-3xl text-default-500`}>Cookie Policy</h2>
         <h2 className={`self-start text-lg mt-6`}>1. Introduction</h2>
         <p className={`text-md text-default-500 mt-2`}>Welcome to Chatify, the web-based Social Media/Chat application.
            This
            Cookie Policy explains how
            we use cookies, similar tracking technologies, and your choices regarding these technologies when you access
            or use our platform.</p>

         <h2 className={`self-start text-lg mt-8`}>2. What Are Cookies?</h2>
         <p className={`text-md text-default-500 text-md mt-2`}>Cookies are small text files that are stored on your
            device (computer,
            smartphone, or any other device) when
            you visit a website or use an application. These files contain information about your browsing activity and
            preferences. Cookies enable the website/application to recognize your device and gather data, such as your
            browsing behavior, preferences, and settings.</p>

         <h2 className={`self-start text-lg mt-8`}>3. How We Use Cookies</h2>
         <p className={`text-md text-md text-default-500 self-start mt-2`}>We utilize cookies for various purposes,
            including but not limited
            to:</p>
         <ul className={`mt-2 list-disc flex flex-col gap-1 self-start`}>
            <CookiePurpose
               title={`Authentication`} description={`Cookies help us authenticate and identify
               users, allowing you to access
               your account securely and efficiently.`} />
            <CookiePurpose
               title={`Security`} description={`We use cookies for security purposes to prevent fraudulent activities and
               enhance the safety of our platform.`} />
            <CookiePurpose
               title={`Performance`} description={`Cookies aid in analyzing the performance and functionality of our
               platform, enabling us to improve user experience and troubleshoot issues.`} />
            <CookiePurpose
               title={`Personalization`} description={`We may use cookies to customize your experience, remember your
               preferences, and provide personalized content and features.`} />
         </ul>
      </section>
   );
};

interface CookiePurposeProps {
   title: string;
   description: string;
}

const CookiePurpose = ({ title, description }: CookiePurposeProps) => {
   return (
      <li className={`text-sm text-default-500 ml-6`}>
         <strong className={`text-default-700`}>{title}:</strong> {description}
      </li>
   );

};

export default Page;
