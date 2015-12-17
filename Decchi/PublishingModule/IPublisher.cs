using Decchi.ParsingModule;

namespace Decchi.PublishingModule
{
	internal interface IDecchiPublisher
	{
		bool Login();

		bool Publish(SongInfo text);
	}
}
